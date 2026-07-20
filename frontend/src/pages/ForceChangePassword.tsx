import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../utils/api';
import { useAuth } from '../contexts/AuthContext';

export default function ForceChangePassword() {
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { updateUser, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmPassword) {
      setError('New passwords do not match.');
      return;
    }

    try {
      setLoading(true);
      setError('');
      const res = await api.post('/auth/force-change-password', {
        newPassword,
        confirmPassword
      });

      if (res.data.success) {
        updateUser({ isPasswordChangeRequired: false });
        navigate('/dashboard');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to change password. Please check your current password.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ display: 'flex', minHeight: '100vh', alignItems: 'center', justifyContent: 'center', padding: '1rem' }}>
      <div className="glass-panel" style={{ padding: '3rem', width: '100%', maxWidth: '420px', textAlign: 'center' }}>
        <div style={{ marginBottom: '2.5rem' }}>
          <img src="/logo.jpeg" alt="FUCHS Logo" style={{ maxWidth: '180px', marginBottom: '1rem' }} />
          <h2 style={{ fontSize: '1.25rem', fontWeight: '600', color: 'var(--text-main)', marginBottom: '0.25rem' }}>Update Required</h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>Please change your temporary password to continue.</p>
        </div>

        {error && <div className="error-text" style={{ marginBottom: '1.5rem' }}>{error}</div>}

        <form onSubmit={handleSubmit} style={{ textAlign: 'left' }}>
          <div className="form-group" style={{ position: 'relative' }}>
            <label>New Password</label>
            <input 
              type={showPassword ? 'text' : 'password'} 
              required 
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              style={{ paddingRight: '2.5rem' }}
            />
            <button 
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              style={{ position: 'absolute', right: '0.75rem', top: '2.1rem', background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)' }}
            >
              {showPassword ? 'Hide' : 'Show'}
            </button>
          </div>
          <div className="form-group" style={{ position: 'relative' }}>
            <label>Confirm New Password</label>
            <input 
              type={showConfirmPassword ? 'text' : 'password'} 
              required 
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              style={{ paddingRight: '2.5rem' }}
            />
            <button 
              type="button"
              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
              style={{ position: 'absolute', right: '0.75rem', top: '2.1rem', background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)' }}
            >
              {showConfirmPassword ? 'Hide' : 'Show'}
            </button>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', marginTop: '1.5rem' }}>
            <button 
              type="submit" 
              className="btn-primary" 
              disabled={loading}
            >
              {loading ? 'Updating...' : 'Update Password'}
            </button>
            <button 
              type="button" 
              className="btn-secondary" 
              onClick={handleLogout}
              disabled={loading}
            >
              Back to Login
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
