import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import api from '../utils/api';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await api.post('/auth/login', { email, password });
      
      if (response.data.success) {
        const data = response.data.data;
        const jwtToken = data.accessToken || data.token || data.AccessToken;
        const { isPasswordChangeRequired, user } = data;
        
        // Pass the actual user object from the backend, mapping roleCode to role for the frontend
        login(jwtToken, { ...user, role: user.roleCode || user.roleName, isPasswordChangeRequired });
        
        if (isPasswordChangeRequired) {
          navigate('/force-change-password');
        } else {
          navigate('/dashboard');
        }
      } else {
        setError(response.data.message || 'Login failed');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'An error occurred during login');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ display: 'flex', minHeight: '100vh', alignItems: 'center', justifyContent: 'center', padding: '1rem' }}>
      <div className="glass-panel" style={{ padding: '3rem', width: '100%', maxWidth: '420px', textAlign: 'center' }}>
        <div style={{ marginBottom: '2.5rem' }}>
          <img src="/logo.jpeg" alt="FUCHS Logo" style={{ maxWidth: '180px', marginBottom: '1rem' }} />
          <h2 style={{ fontSize: '1.25rem', fontWeight: '600', color: 'var(--text-main)', marginBottom: '0.25rem' }}>ERMS Portal</h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>Expense Reimbursement Management System</p>
        </div>

        <form onSubmit={handleSubmit} style={{ textAlign: 'left' }}>
          <div className="form-group">
            <label htmlFor="email">Email Address</label>
            <input 
              id="email"
              type="email" 
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="name@company.com"
              required 
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input 
              id="password"
              type="password" 
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required 
            />
          </div>

          {error && <div className="error-text" style={{ marginBottom: '1rem' }}>{error}</div>}

          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>
      </div>
    </div>
  );
}
