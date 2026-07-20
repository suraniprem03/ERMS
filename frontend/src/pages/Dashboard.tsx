import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import api from '../utils/api';

interface Claim {
  id: number;
  claimNumber: string;
  expenseCategoryName: string;
  totalAmount: number;
  statusName: string;
  submittedAt?: string;
  employeeName: string;
}

export default function Dashboard() {
  const { user } = useAuth();
  const [claims, setClaims] = useState<Claim[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchClaims = async () => {
      try {
        const endpoint = (user?.role === 'SUPER_ADMIN' || user?.role === 'ADMIN') ? '/claims/all' : '/claims/my';
        const res = await api.get(endpoint);
        if (res.data.success) {
          setClaims(res.data.data);
        }
      } catch (err) {
        console.error('Failed to load dashboard stats');
      } finally {
        setLoading(false);
      }
    };
    fetchClaims();
  }, []);

  const pendingClaims = claims.filter(c => ['Draft', 'Submitted'].includes(c.statusName)).length;
  const approvedClaims = claims.filter(c => c.statusName === 'Approved').length;
  const rejectedClaims = claims.filter(c => c.statusName === 'Rejected').length;
  const totalReimbursed = claims.filter(c => c.statusName === 'Approved').reduce((sum, c) => sum + (c.totalAmount || 0), 0);

  return (
    <div>
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ fontSize: '1.875rem', fontWeight: 'bold', color: 'var(--text-main)' }}>Dashboard</h1>
        <p style={{ color: 'var(--text-muted)' }}>Welcome back, {user?.name}. Here's an overview of your claims.</p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem' }}>
        <div className="stat-card">
          <h3>Pending Claims</h3>
          <div className="value">{loading ? '...' : pendingClaims}</div>
        </div>
        
        <div className="stat-card">
          <h3>Approved Claims</h3>
          <div className="value" style={{ color: 'var(--success, #10b981)' }}>{loading ? '...' : approvedClaims}</div>
        </div>

        <div className="stat-card">
          <h3>Rejected Claims</h3>
          <div className="value" style={{ color: 'var(--error, #ef4444)' }}>{loading ? '...' : rejectedClaims}</div>
        </div>

        <div className="stat-card">
          <h3>Total Reimbursed</h3>
          <div className="value">{loading ? '...' : `₹${totalReimbursed.toFixed(2)}`}</div>
        </div>
      </div>
      
      <div className="glass-panel" style={{ marginTop: '2rem', padding: '1.5rem' }}>
        <h3 style={{ marginBottom: '1rem', color: 'var(--text-main)' }}>Recent Activity</h3>
        {loading ? (
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>Loading activity...</p>
        ) : claims.length === 0 ? (
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>No recent claims found. Start by submitting a new expense claim.</p>
        ) : (
          <ul style={{ listStyleType: 'none' }}>
            {claims.slice(0, 5).map(c => (
              <li key={c.id} style={{ padding: '0.75rem 0', borderBottom: '1px solid var(--border-color)', display: 'flex', justifyContent: 'space-between' }}>
                <div>
                  <span style={{ fontWeight: '600' }}>{c.claimNumber}</span> - {c.expenseCategoryName}
                </div>
                <div style={{ fontWeight: '600', color: c.statusName === 'Approved' ? 'var(--primary-color)' : 'inherit' }}>
                  ₹{(c.totalAmount || 0).toFixed(2)}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
