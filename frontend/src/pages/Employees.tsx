import { useState, useEffect } from 'react';
import api from '../utils/api';

interface Employee {
  id: number;
  name: string;
  email: string;
  employeeCode: string;
  mobileNumber: string;
  roleName: string;
  assignedAreaIds: number[];
  isActive: boolean;
}

export default function Employees() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingEmployeeId, setEditingEmployeeId] = useState<number | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    mobileNumber: '',
    areaId: '',
    isActive: true
  });
  
  // Form Dropdown Data
  const [areas, setAreas] = useState<{id: number, name: string}[]>([]);

  const fetchEmployees = async () => {
    try {
      setLoading(true);
      const res = await api.get('/employees');
      if (res.data.success) {
        setEmployees(res.data.data);
      }
    } catch (err) {
      setError('Failed to load employees.');
    } finally {
      setLoading(false);
    }
  };

  const fetchAreas = async () => {
    try {
      const res = await api.get('/areas');
      if (res.data.success) {
        setAreas(res.data.data);
      }
    } catch (err) {
      console.error('Failed to load areas for dropdown');
    }
  };

  useEffect(() => {
    fetchEmployees();
    fetchAreas();
  }, []);

  const handleCreateOrUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const payload: any = {
        name: formData.name,
        mobileNumber: formData.mobileNumber,
        areaIds: [parseInt(formData.areaId)],
        isActive: formData.isActive
      };
      
      let res;
      if (editingEmployeeId) {
        res = await api.put(`/employees/${editingEmployeeId}`, payload);
      } else {
        payload.email = formData.email;
        res = await api.post('/employees', payload);
      }
      
      if (res.data.success) {
        setIsModalOpen(false);
        setEditingEmployeeId(null);
        setFormData({ name: '', email: '', mobileNumber: '', areaId: '', isActive: true });
        fetchEmployees(); // Refresh table
      } else {
        alert(res.data.message || 'Operation failed');
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'An error occurred');
    }
  };
  
  const openEdit = (emp: Employee) => {
    setEditingEmployeeId(emp.id);
    setFormData({
      name: emp.name,
      email: emp.email, // read-only on edit
      mobileNumber: emp.mobileNumber || '',
      areaId: emp.assignedAreaIds && emp.assignedAreaIds.length > 0 ? emp.assignedAreaIds[0].toString() : '',
      isActive: emp.isActive
    });
    setIsModalOpen(true);
  };
  
  const openCreate = () => {
    setEditingEmployeeId(null);
    setFormData({ name: '', email: '', mobileNumber: '', areaId: '', isActive: true });
    setIsModalOpen(true);
  };

  const handleResetPassword = async (id: number) => {
    if (!window.confirm('Are you sure you want to reset this employee\'s password to "Reset!Password123"? They will be forced to change it on their next login.')) {
      return;
    }
    
    try {
      const res = await api.put(`/employees/${id}/reset-password`);
      if (res.data.success) {
        alert(res.data.message);
      } else {
        alert(res.data.message || 'Failed to reset password.');
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'An error occurred while resetting password.');
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h1 style={{ fontSize: '1.875rem', fontWeight: 'bold', color: 'var(--text-main)' }}>Employees</h1>
          <p style={{ color: 'var(--text-muted)' }}>Manage employee accounts and roles.</p>
        </div>
        <button className="btn-primary" style={{ width: 'auto' }} onClick={openCreate}>
          + Add Employee
        </button>
      </div>

      {error && <div className="error-text" style={{ marginBottom: '1rem' }}>{error}</div>}

      <div className="table-container">
        {loading ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-muted)' }}>Loading...</div>
        ) : (
          <table className="premium-table">
            <thead>
              <tr>
                <th>Code</th>
                <th>Name</th>
                <th>Mobile</th>
                <th>Email</th>
                <th>Role</th>
                <th>Area</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {employees.length === 0 ? (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-muted)' }}>No employees found.</td>
                </tr>
              ) : (
                employees.map(emp => (
                  <tr key={emp.id}>
                    <td style={{ fontWeight: '500' }}>{emp.employeeCode}</td>
                    <td>{emp.name}</td>
                    <td>{emp.mobileNumber || '-'}</td>
                    <td>{emp.email}</td>
                    <td>
                      <span className={`badge ${emp.roleName === 'ADMIN' ? 'badge-blue' : 'badge-green'}`}>
                        {emp.roleName}
                      </span>
                    </td>
                    <td>{areas.find(a => a.id === emp.assignedAreaIds?.[0])?.name || '-'}</td>
                    <td>
                      <span className={`badge ${emp.isActive ? 'badge-green' : 'badge-red'}`}>
                        {emp.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: '0.75rem' }}>
                        <button style={{background:'none',border:'none',color:'var(--primary-color)',cursor:'pointer',fontWeight:'600'}} onClick={() => openEdit(emp)}>Edit</button>
                        <button style={{background:'none',border:'none',color:'var(--error)',cursor:'pointer',fontWeight:'600'}} onClick={() => handleResetPassword(emp.id)}>Reset Pass</button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        )}
      </div>

      {/* Create/Edit Modal */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={() => setIsModalOpen(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2 style={{ color: 'var(--text-main)', marginBottom: '1.5rem', fontSize: '1.5rem' }}>
              {editingEmployeeId ? 'Edit Employee' : 'Create Employee'}
            </h2>
            <form onSubmit={handleCreateOrUpdate}>
              <div className="form-group-grid">
                <div className="form-group">
                  <label>Full Name</label>
                  <input required value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} />
                </div>
                
                <div className="form-group">
                  <label>Mobile Number</label>
                  <input required value={formData.mobileNumber} onChange={e => setFormData({...formData, mobileNumber: e.target.value})} />
                </div>
              </div>
              
              <div className="form-group-grid">
                <div className="form-group">
                  <label>Email {editingEmployeeId && '(Cannot be changed)'}</label>
                  <input type="email" required readOnly={!!editingEmployeeId} style={{ backgroundColor: editingEmployeeId ? '#e2e8f0' : 'white' }} value={formData.email} onChange={e => setFormData({...formData, email: e.target.value})} />
                </div>

                <div className="form-group">
                  <label>Area</label>
                  <select 
                    required 
                    value={formData.areaId} 
                    onChange={e => setFormData({...formData, areaId: e.target.value})}
                  >
                    <option value="">Select Area...</option>
                    {areas.map(a => <option key={a.id} value={a.id}>{a.name}</option>)}
                  </select>
                </div>
              </div>

              {editingEmployeeId && (
                <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                  <input type="checkbox" id="isActive" checked={formData.isActive} onChange={e => setFormData({...formData, isActive: e.target.checked})} />
                  <label htmlFor="isActive" style={{ margin: 0 }}>Is Active</label>
                </div>
              )}

              <div style={{ display: 'flex', gap: '1rem', marginTop: '2rem' }}>
                <button type="button" className="btn-secondary" onClick={() => setIsModalOpen(false)} style={{ flex: 1 }}>
                  Cancel
                </button>
                <button type="submit" className="btn-primary" style={{ flex: 1 }}>
                  {editingEmployeeId ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
