import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { VulnerabilitiesPage } from './pages/VulnerabilitiesPage';
import { VulnerabilityDetailPage } from './pages/VulnerabilityDetailPage';
import { FileVulnerabilityPage } from './pages/FileVulnerabilityPage';
import { AssetsPage } from './pages/AssetsPage';
import { AppLayout } from './components/common/AppLayout';

const DEV_AUTH = import.meta.env.VITE_DEV_AUTH === 'true';

function AppRoutes() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/vulnerabilities" element={<VulnerabilitiesPage />} />
        <Route path="/vulnerabilities/:id" element={<VulnerabilityDetailPage />} />
        <Route path="/file-vulnerability" element={<FileVulnerabilityPage />} />
        <Route path="/assets" element={<AssetsPage />} />
      </Route>
    </Routes>
  );
}

export function App() {
  if (DEV_AUTH) {
    return (
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    );
  }

  return (
    <BrowserRouter>
      <UnauthenticatedTemplate>
        <Routes>
          <Route path="*" element={<LoginPage />} />
        </Routes>
      </UnauthenticatedTemplate>
      <AuthenticatedTemplate>
        <AppRoutes />
      </AuthenticatedTemplate>
    </BrowserRouter>
  );
}
