import { Outlet, NavLink } from 'react-router-dom';
import { useMsal } from '@azure/msal-react';

const DEV_AUTH = import.meta.env.VITE_DEV_AUTH === 'true';

const NAV_ITEMS = [
  {
    to: '/dashboard',
    label: 'Home',
    icon: (
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
          d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
      </svg>
    ),
  },
  {
    to: '/vulnerabilities',
    label: 'Inventory',
    icon: (
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
          d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
      </svg>
    ),
  },
  {
    to: '/file-vulnerability',
    label: 'File Vulnerability',
    icon: (
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
          d="M9 13h6m-3-3v6m5 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
    ),
  },
  {
    to: '/assets',
    label: 'Assets',
    icon: (
      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
          d="M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2m-2-4h.01M17 16h.01" />
      </svg>
    ),
  },
];

function ProdUserInfo() {
  const { instance, accounts } = useMsal();
  const user = accounts[0];
  return (
    <div className="border-t border-gray-800 px-5 py-4">
      <p className="truncate text-xs font-medium text-gray-300">{user?.name ?? user?.username}</p>
      <p className="truncate text-xs text-gray-500">{user?.username}</p>
      <button
        onClick={() => instance.logoutRedirect()}
        className="mt-2 text-xs text-gray-500 hover:text-gray-300"
      >
        Sign out
      </button>
    </div>
  );
}

function DevUserInfo() {
  return (
    <div className="border-t border-gray-800 px-5 py-4">
      <p className="truncate text-xs font-medium text-gray-300">Dev User</p>
      <p className="truncate text-xs text-gray-500">dev@vulntrack.local</p>
      <p className="mt-1 text-xs text-yellow-500">Dev mode</p>
    </div>
  );
}

export function AppLayout() {
  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      <aside className="flex w-60 shrink-0 flex-col bg-gray-900 text-gray-100">
        <div className="flex items-center gap-2 px-5 py-5">
          <span className="text-lg font-bold tracking-tight">VulnTrack</span>
        </div>

        <nav className="flex-1 space-y-0.5 px-3 py-2">
          {NAV_ITEMS.map(({ to, label, icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-blue-600 text-white'
                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                }`
              }
            >
              {icon}
              {label}
            </NavLink>
          ))}
        </nav>

        {DEV_AUTH ? <DevUserInfo /> : <ProdUserInfo />}
      </aside>

      <main className="flex-1 overflow-auto p-8">
        <Outlet />
      </main>
    </div>
  );
}
