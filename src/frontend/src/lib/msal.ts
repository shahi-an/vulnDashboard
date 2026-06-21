import { PublicClientApplication, Configuration, LogLevel } from '@azure/msal-browser';

const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID}`,
    redirectUri: import.meta.env.VITE_AZURE_REDIRECT_URI ?? window.location.origin,
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: true,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return;
        if (level === LogLevel.Error) console.error('[MSAL]', message);
        if (level === LogLevel.Warning) console.warn('[MSAL]', message);
        if (level === LogLevel.Info) console.info('[MSAL]', message);
        if (level === LogLevel.Verbose) console.debug('[MSAL]', message);
      },
      logLevel: LogLevel.Info,
    },
  },
};

export const msalInstance = new PublicClientApplication(msalConfig);

export const loginRequest = {
  scopes: [import.meta.env.VITE_API_SCOPE ?? 'openid', 'profile'],
};
