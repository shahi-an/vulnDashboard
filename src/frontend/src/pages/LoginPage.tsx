import { useMsal } from '@azure/msal-react';
import { loginRequest } from '@/lib/msal';

export function LoginPage() {
  const { instance } = useMsal();

  const handleLogin = () => instance.loginRedirect(loginRequest);

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50">
      <div className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-lg">
        <h1 className="mb-2 text-2xl font-bold text-gray-900">VulnTrack</h1>
        <p className="mb-8 text-sm text-gray-500">Vulnerability Management Portal</p>
        <button
          onClick={handleLogin}
          className="w-full rounded-lg bg-blue-600 px-4 py-3 text-sm font-semibold text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
        >
          Sign in with Microsoft
        </button>
      </div>
    </div>
  );
}
