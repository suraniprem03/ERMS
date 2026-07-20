import { useState, useEffect } from 'react';
import api from '../utils/api';
import { useAuth } from '../contexts/AuthContext';

export default function Settings() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<'states' | 'areas' | 'categories' | 'security'>('states');
  
  const [states, setStates] = useState<any[]>([]);
  const [areas, setAreas] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  // Modals
  const [isStateModalOpen, setIsStateModalOpen] = useState(false);
  const [isAreaModalOpen, setIsAreaModalOpen] = useState(false);
  const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false);
  const [editingCategoryId, setEditingCategoryId] = useState<number | null>(null);

  // Forms
  const [stateForm, setStateForm] = useState({ name: '', code: '' });
  const [areaForm, setAreaForm] = useState({ stateId: '', name: '', code: '' });
  const [categoryForm, setCategoryForm] = useState({ name: '', code: '', maxLimit: '', isDynamic: false });
  const [passwordForm, setPasswordForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [passwordError, setPasswordError] = useState('');
  const [passwordSuccess, setPasswordSuccess] = useState('');

  useEffect(() => {
    fetchData();
  }, [activeTab]);

  const fetchData = async () => {
    setLoading(true);
    try {
      if (activeTab === 'states') {
        const res = await api.get('/states');
        if (res.data.success) setStates(res.data.data);
      } else if (activeTab === 'areas') {
        const res = await api.get('/areas');
        if (res.data.success) setAreas(res.data.data);
        // Also fetch states for the area dropdown
        if (states.length === 0) {
          const stateRes = await api.get('/states');
          if (stateRes.data.success) setStates(stateRes.data.data);
        }
      } else if (activeTab === 'categories') {
        const res = await api.get('/expense-categories');
        if (res.data.success) setCategories(res.data.data);
      }
    } catch (err) {
      console.error('Failed to fetch data', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateState = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await api.post('/states', stateForm);
      if (res.data.success) {
        setIsStateModalOpen(false);
        setStateForm({ name: '', code: '' });
        fetchData();
      } else alert(res.data.message);
    } catch (err: any) { alert(err.response?.data?.message || 'Error'); }
  };

  const handleCreateArea = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await api.post('/areas', { ...areaForm, stateId: parseInt(areaForm.stateId) });
      if (res.data.success) {
        setIsAreaModalOpen(false);
        setAreaForm({ stateId: '', name: '', code: '' });
        fetchData();
      } else alert(res.data.message);
    } catch (err: any) { alert(err.response?.data?.message || 'Error'); }
  };

  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const payload = {
        ...categoryForm,
        maxLimit: parseFloat(categoryForm.maxLimit) || null
      };
      
      let res;
      if (editingCategoryId) {
        res = await api.put(`/expense-categories/${editingCategoryId}`, payload);
      } else {
        res = await api.post('/expense-categories', payload);
      }
      
      if (res.data.success) {
        setIsCategoryModalOpen(false);
        setEditingCategoryId(null);
        setCategoryForm({ name: '', code: '', maxLimit: '', isDynamic: false });
        fetchData();
      } else alert(res.data.message);
    } catch (err: any) { alert(err.response?.data?.message || 'Error'); }
  };
  
  const openEditCategory = (category: any) => {
    setEditingCategoryId(category.id);
    setCategoryForm({
      name: category.name,
      code: category.code,
      maxLimit: category.maxLimit ? category.maxLimit.toString() : '',
      isDynamic: category.isDynamic
    });
    setIsCategoryModalOpen(true);
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setPasswordError('New passwords do not match');
      return;
    }
    try {
      setLoading(true);
      setPasswordError('');
      setPasswordSuccess('');
      const res = await api.post('/auth/change-password', {
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword
      });
      if (res.data.success) {
        setPasswordSuccess('Password changed successfully!');
        setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
      } else {
        setPasswordError(res.data.message);
      }
    } catch (err: any) {
      setPasswordError(err.response?.data?.message || 'Failed to change password');
    } finally {
      setLoading(false);
    }
  };

  if (user?.role !== 'ADMIN' && user?.role !== 'SUPER_ADMIN') {
    return <div style={{ padding: '2rem' }}>You do not have permission to view this page.</div>;
  }

  return (
    <div>
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ fontSize: '1.875rem', fontWeight: 'bold', color: 'var(--text-main)' }}>Master Settings</h1>
        <p style={{ color: 'var(--text-muted)' }}>Manage States, Areas, and Expense Categories.</p>
      </div>

      <div style={{ display: 'flex', gap: '1rem', borderBottom: '1px solid var(--border-color)', marginBottom: '2rem' }}>
        <button 
          onClick={() => setActiveTab('states')}
          style={{ background: 'none', border: 'none', padding: '0.75rem 1rem', cursor: 'pointer', fontWeight: '600', color: activeTab === 'states' ? 'var(--primary-color)' : 'var(--text-muted)', borderBottom: activeTab === 'states' ? '2px solid var(--primary-color)' : '2px solid transparent' }}
        >
          States
        </button>
        <button 
          onClick={() => setActiveTab('areas')}
          style={{ background: 'none', border: 'none', padding: '0.75rem 1rem', cursor: 'pointer', fontWeight: '600', color: activeTab === 'areas' ? 'var(--primary-color)' : 'var(--text-muted)', borderBottom: activeTab === 'areas' ? '2px solid var(--primary-color)' : '2px solid transparent' }}
        >
          Areas
        </button>
        <button 
          onClick={() => setActiveTab('categories')}
          style={{ background: 'none', border: 'none', padding: '0.75rem 1rem', cursor: 'pointer', fontWeight: '600', color: activeTab === 'categories' ? 'var(--primary-color)' : 'var(--text-muted)', borderBottom: activeTab === 'categories' ? '2px solid var(--primary-color)' : '2px solid transparent' }}
        >
          Expense Categories
        </button>
        <button 
          onClick={() => setActiveTab('security')}
          style={{ background: 'none', border: 'none', padding: '0.75rem 1rem', cursor: 'pointer', fontWeight: '600', color: activeTab === 'security' ? 'var(--primary-color)' : 'var(--text-muted)', borderBottom: activeTab === 'security' ? '2px solid var(--primary-color)' : '2px solid transparent' }}
        >
          Security
        </button>
      </div>

      {loading ? (
        <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-muted)' }}>Loading...</div>
      ) : (
        <div className="table-container">
          {activeTab === 'states' && (
            <>
              <div style={{ padding: '1rem', display: 'flex', justifyContent: 'flex-end' }}>
                <button className="btn-primary" style={{ width: 'auto' }} onClick={() => setIsStateModalOpen(true)}>+ Add State</button>
              </div>
              <table className="premium-table">
                <thead><tr><th>ID</th><th>Code</th><th>Name</th></tr></thead>
                <tbody>
                  {states.map(s => <tr key={s.id}><td>{s.id}</td><td style={{ fontWeight: '600' }}>{s.code}</td><td>{s.name}</td></tr>)}
                  {states.length === 0 && <tr><td colSpan={3} style={{ textAlign: 'center' }}>No states found.</td></tr>}
                </tbody>
              </table>
            </>
          )}

          {activeTab === 'areas' && (
            <>
              <div style={{ padding: '1rem', display: 'flex', justifyContent: 'flex-end' }}>
                <button className="btn-primary" style={{ width: 'auto' }} onClick={() => setIsAreaModalOpen(true)}>+ Add Area</button>
              </div>
              <table className="premium-table">
                <thead><tr><th>ID</th><th>Code</th><th>Name</th><th>State</th></tr></thead>
                <tbody>
                  {areas.map(a => <tr key={a.id}><td>{a.id}</td><td style={{ fontWeight: '600' }}>{a.code}</td><td>{a.name}</td><td>{a.stateName}</td></tr>)}
                  {areas.length === 0 && <tr><td colSpan={4} style={{ textAlign: 'center' }}>No areas found.</td></tr>}
                </tbody>
              </table>
            </>
          )}

          {activeTab === 'categories' && (
            <>
              <div style={{ padding: '1rem', display: 'flex', justifyContent: 'flex-end' }}>
                <button className="btn-primary" style={{ width: 'auto' }} onClick={() => { setEditingCategoryId(null); setCategoryForm({ name: '', code: '', maxLimit: '', isDynamic: false }); setIsCategoryModalOpen(true); }}>+ Add Category</button>
              </div>
              <table className="premium-table">
                <thead><tr><th>ID</th><th>Code</th><th>Name</th><th>Max Limit</th><th>Is Dynamic</th><th>Actions</th></tr></thead>
                <tbody>
                  {categories.map(c => <tr key={c.id}><td>{c.id}</td><td style={{ fontWeight: '600' }}>{c.code}</td><td>{c.name}</td><td>{c.maxLimit ? `₹${c.maxLimit}` : 'Unlimited'}</td><td>{c.isDynamic ? 'Yes' : 'No'}</td><td><button style={{background:'none',border:'none',color:'var(--primary-color)',cursor:'pointer',fontWeight:'600'}} onClick={()=>openEditCategory(c)}>Edit</button></td></tr>)}
                  {categories.length === 0 && <tr><td colSpan={6} style={{ textAlign: 'center' }}>No categories found.</td></tr>}
                </tbody>
              </table>
            </>
          )}

          {activeTab === 'security' && (
            <div style={{ padding: '2rem', maxWidth: '500px', margin: '0 auto' }}>
              <h2 style={{ marginBottom: '1.5rem', color: 'var(--text-main)' }}>Change Password</h2>
              {passwordError && <div className="error-text" style={{ marginBottom: '1rem' }}>{passwordError}</div>}
              {passwordSuccess && <div style={{ color: 'var(--primary-color)', marginBottom: '1rem', fontWeight: '500' }}>{passwordSuccess}</div>}
              <form onSubmit={handleChangePassword}>
                <div className="form-group">
                  <label>Current Password</label>
                  <input type="password" required value={passwordForm.currentPassword} onChange={e => setPasswordForm({...passwordForm, currentPassword: e.target.value})} />
                </div>
                <div className="form-group">
                  <label>New Password</label>
                  <input type="password" required value={passwordForm.newPassword} onChange={e => setPasswordForm({...passwordForm, newPassword: e.target.value})} />
                </div>
                <div className="form-group">
                  <label>Confirm New Password</label>
                  <input type="password" required value={passwordForm.confirmPassword} onChange={e => setPasswordForm({...passwordForm, confirmPassword: e.target.value})} />
                </div>
                <button type="submit" className="btn-primary" disabled={loading}>Update Password</button>
              </form>
            </div>
          )}
        </div>
      )}

      {/* State Modal */}
      {isStateModalOpen && (
        <div className="modal-overlay" onClick={() => setIsStateModalOpen(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2 style={{ marginBottom: '1.5rem' }}>Add State</h2>
            <form onSubmit={handleCreateState}>
              <div className="form-group"><label>Name</label><input required value={stateForm.name} onChange={e => setStateForm({...stateForm, name: e.target.value})} /></div>
              <div className="form-group"><label>Code (e.g. MH, GJ)</label><input required value={stateForm.code} onChange={e => setStateForm({...stateForm, code: e.target.value})} /></div>
              <button type="submit" className="btn-primary">Save State</button>
            </form>
          </div>
        </div>
      )}

      {/* Area Modal */}
      {isAreaModalOpen && (
        <div className="modal-overlay" onClick={() => setIsAreaModalOpen(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2 style={{ marginBottom: '1.5rem' }}>Add Area</h2>
            <form onSubmit={handleCreateArea}>
              <div className="form-group"><label>State</label>
                <select required value={areaForm.stateId} onChange={e => setAreaForm({...areaForm, stateId: e.target.value})}>
                  <option value="">Select State...</option>
                  {states.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                </select>
              </div>
              <div className="form-group"><label>Name</label><input required value={areaForm.name} onChange={e => setAreaForm({...areaForm, name: e.target.value})} /></div>
              <div className="form-group"><label>Code</label><input required value={areaForm.code} onChange={e => setAreaForm({...areaForm, code: e.target.value})} /></div>
              <button type="submit" className="btn-primary">Save Area</button>
            </form>
          </div>
        </div>
      )}

      {/* Category Modal */}
      {isCategoryModalOpen && (
        <div className="modal-overlay" onClick={() => setIsCategoryModalOpen(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2 style={{ marginBottom: '1.5rem' }}>{editingCategoryId ? 'Edit Expense Category' : 'Add Expense Category'}</h2>
            <form onSubmit={handleCreateCategory}>
              <div className="form-group"><label>Name</label><input required value={categoryForm.name} onChange={e => setCategoryForm({...categoryForm, name: e.target.value})} /></div>
              <div className="form-group"><label>Code (e.g. TRAVEL, HOTEL)</label><input required value={categoryForm.code} onChange={e => setCategoryForm({...categoryForm, code: e.target.value})} /></div>
              <div className="form-group"><label>Max Limit (Optional)</label><input type="number" step="0.01" value={categoryForm.maxLimit} onChange={e => setCategoryForm({...categoryForm, maxLimit: e.target.value})} /></div>
              <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <input type="checkbox" id="isDynamic" checked={categoryForm.isDynamic} onChange={e => setCategoryForm({...categoryForm, isDynamic: e.target.checked})} />
                <label htmlFor="isDynamic" style={{ margin: 0 }}>Is Dynamic (Form changes based on this)</label>
              </div>
              <button type="submit" className="btn-primary" style={{ marginTop: '1rem' }}>Save Category</button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
