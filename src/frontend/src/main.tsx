import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { MsalProvider } from '@azure/msal-react';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { EventType } from '@azure/msal-browser';
import { App } from './App';
import { msalInstance } from './lib/msal';
import { queryClient } from './lib/queryClient';
import './index.css';

msalInstance.addEventCallback((event) => {
  console.info('[MSAL event]', event.eventType, event);
  if (event.error) {
    console.error('[MSAL error]', event.eventType, event.error);
  }
  if (event.eventType === EventType.LOGIN_SUCCESS || event.eventType === EventType.ACQUIRE_TOKEN_SUCCESS) {
    console.info('[MSAL] Auth success — accounts:', msalInstance.getAllAccounts().length);
  }
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MsalProvider instance={msalInstance}>
      <QueryClientProvider client={queryClient}>
        <App />
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </MsalProvider>
  </StrictMode>,
);
