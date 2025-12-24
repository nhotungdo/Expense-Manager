import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from '@store/authStore';
import Layout from '@components/Layout/Layout';
import Dashboard from '@pages/Dashboard';
import Users from '@pages/Users';
import Transactions from '@pages/Transactions';
import ServicePackages from '@pages/ServicePackages';
import Categories from '@pages/Categories';
import SystemSettings from '@pages/SystemSettings';
import AuditLogs from '@pages/AuditLogs';
import Login from '@pages/Login';
import NotFound from '@pages/NotFound';

function App() {
  const { isAuthenticated } = useAuthStore();

  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      
      <Route
        path="/"
        element={isAuthenticated ? <Layout /> : <Navigate to="/login" replace />}
      >
        <Route index element={<Dashboard />} />
        <Route path="users" element={<Users />} />
        <Route path="transactions" element={<Transactions />} />
        <Route path="service-packages" element={<ServicePackages />} />
        <Route path="categories" element={<Categories />} />
        <Route path="settings" element={<SystemSettings />} />
        <Route path="audit-logs" element={<AuditLogs />} />
      </Route>

      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

export default App;
