import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export default function ProtectedRoute() {
  const { token, user } = useAuth();
  const location = useLocation();

  if (!token) {
    // If not authenticated, redirect to login page
    return <Navigate to="/login" replace />;
  }

  // If user data is still loading (token exists but user is null), we can show a loader or just wait
  if (token && !user) {
    return <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', color: 'var(--text-muted)' }}>Loading session...</div>;
  }

  if (user?.isPasswordChangeRequired && location.pathname !== '/force-change-password') {
    return <Navigate to="/force-change-password" replace />;
  }

  // If authenticated, render child routes
  return <Outlet />;
}
