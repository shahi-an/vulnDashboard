import { cn } from '@/lib/cn';

interface SpinnerProps {
  className?: string;
}

export function Spinner({ className }: SpinnerProps) {
  return (
    <div
      className={cn(
        'h-5 w-5 animate-spin rounded-full border-2 border-gray-300 border-t-blue-600',
        className,
      )}
    />
  );
}
