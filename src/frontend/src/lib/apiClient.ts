import axios from 'axios';
import { msalInstance, loginRequest } from './msal';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
  headers: { 'Content-Type': 'application/json' },
});

const DEV_AUTH = import.meta.env.VITE_DEV_AUTH === 'true';

// Attach Bearer token on every request (skipped in dev-auth bypass mode)
apiClient.interceptors.request.use(async (config) => {
  if (DEV_AUTH) return config;

  const account = msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0];

  if (account) {
    const { accessToken } = await msalInstance.acquireTokenSilent({
      ...loginRequest,
      account,
    });
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  return config;
});
