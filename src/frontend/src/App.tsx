import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { VulnerabilitiesPage } from './pages/VulnerabilitiesPage';
import { AssetsPage } from './pages/AssetsPage';
import { AppLayout } from './components/common/AppLayout';

export function App() {
  return (
    <BrowserRouter>
      <UnauthenticatedTemplate>
        <Routes>
          <Route path="*" element={<LoginPage />} />
        </Routes>
      </UnauthenticatedTemplate>

      <AuthenticatedTemplate>
        <Routes>
          <Route element={<AppLayout />}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/vulnerabilities" element={<VulnerabilitiesPage />} />
            <Route path="/assets" element={<AssetsPage />} />
          </Route>
        </Routes>
      </AuthenticatedTemplate>
    </BrowserRouter>
  );
}
