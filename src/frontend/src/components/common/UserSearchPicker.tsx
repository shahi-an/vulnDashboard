import { useState, useRef, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { userService } from '@/services/userService';

export interface PickedUser {
  userId?: string;
  email: string;
  displayName?: string;
}

interface UserSearchPickerProps {
  value: PickedUser | null;
  onChange: (user: PickedUser | null) => void;
  onInputChange?: (text: string) => void;
  placeholder?: string;
  className?: string;
}

export function UserSearchPicker({
  value,
  onChange,
  onInputChange,
  placeholder = 'Search by name or email…',
  className = '',
}: UserSearchPickerProps) {
  const [inputText, setInputText] = useState(value ? (value.displayName ?? value.email) : '');
  const [debouncedQuery, setDebouncedQuery] = useState('');
  const [open, setOpen] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout>>();

  useEffect(() => {
    setInputText(value ? (value.displayName ?? value.email) : '');
  }, [value]);

  const { data: suggestions = [] } = useQuery({
    queryKey: ['users', 'search', debouncedQuery],
    queryFn: () => userService.search(debouncedQuery),
    enabled: debouncedQuery.length >= 2,
  });

  const handleInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    const text = e.target.value;
    setInputText(text);
    onChange(null);
    onInputChange?.(text);
    clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      setDebouncedQuery(text);
      setOpen(true);
    }, 300);
  };

  const handleSelect = (u: (typeof suggestions)[0]) => {
    const picked: PickedUser = {
      userId: u.id,
      email: u.email ?? '',
      displayName: u.displayName,
    };
    onChange(picked);
    onInputChange?.('');
    setInputText(u.displayName ?? u.email ?? '');
    setDebouncedQuery('');
    setOpen(false);
  };

  const handleBlur = () => {
    setTimeout(() => {
      setOpen(false);
      if (!value && inputText.trim()) {
        onChange({ email: inputText.trim() });
      }
    }, 150);
  };

  const handleClear = () => {
    onChange(null);
    onInputChange?.('');
    setInputText('');
    setDebouncedQuery('');
  };

  const showDropdown = open && suggestions.length > 0;

  return (
    <div className={`relative ${className}`}>
      <div className="relative">
        <input
          type="text"
          value={inputText}
          onChange={handleInput}
          onFocus={() => debouncedQuery.length >= 2 && setOpen(true)}
          onBlur={handleBlur}
          placeholder={placeholder}
          className="w-full rounded-lg border border-gray-300 px-3 py-2 pr-8 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        />
        {(value || inputText) && (
          <button
            type="button"
            onMouseDown={(e) => { e.preventDefault(); handleClear(); }}
            className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        )}
      </div>

      {showDropdown && (
        <div className="absolute z-20 mt-1 w-full overflow-hidden rounded-lg border border-gray-200 bg-white shadow-lg">
          {suggestions.map((u) => (
            <button
              key={u.id}
              type="button"
              onMouseDown={(e) => { e.preventDefault(); handleSelect(u); }}
              className="flex w-full flex-col px-3 py-2 text-left hover:bg-blue-50"
            >
              <span className="text-sm font-medium text-gray-900">
                {u.displayName ?? u.email}
              </span>
              {u.displayName && u.email && (
                <span className="text-xs text-gray-500">{u.email}</span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
