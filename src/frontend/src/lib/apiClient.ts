import axios from 'axios';
import { msalInstance, loginRequest } from './msal';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
  headers: { 'Content-Type': 'application/json' },
});

// Attach Bearer token on every request
apiClient.interceptors.request.use(async (config) => {
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
