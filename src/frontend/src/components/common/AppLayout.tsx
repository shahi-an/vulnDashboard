import { Outlet, NavLink } from 'react-router-dom';
import { useMsal } from '@azure/msal-react';

const navItems = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/vulnerabilities', label: 'Vulnerabilities' },
  { to: '/assets', label: 'Assets' },
];

export function AppLayout() {
  const { instance, accounts } = useMsal();
  const user = accounts[0];

  return (
    <div className="flex h-screen overflow-hidden">
      {/* Sidebar */}
      <aside className="flex w-60 flex-col bg-gray-900 text-gray-100">
        <div className="px-6 py-5 text-lg font-bold tracking-tight">VulnTrack</div>
        <nav className="flex-1 space-y-1 px-3 py-4">
          {navItems.map(({ to, label }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `block rounded-lg px-4 py-2.5 text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-blue-600 text-white'
                    : 'text-gray-300 hover:bg-gray-700 hover:text-white'
                }`
              }
            >
              {label}
            </NavLink>
          ))}
        </nav>
        <div className="border-t border-gray-700 px-6 py-4 text-xs text-gray-400">
          <p className="truncate">{user?.name ?? user?.username}</p>
          <button
            onClick={() => instance.logoutRedirect()}
            className="mt-1 text-gray-500 hover:text-gray-300"
          >
            Sign out
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-auto bg-gray-50 p-8">
        <Outlet />
      </main>
    </div>
  );
}
