import { useState, useEffect } from 'react';
import api from '../utils/api';
import { useAuth } from '../contexts/AuthContext';
import ClaimDetailsModal from '../components/ClaimDetailsModal';

interface Claim {
  id: number;
  claimNumber: string;
  expenseCategoryName: string;
  totalAmount: number;
  statusName: string;
  submittedAt?: string;
  claimDate?: string;
  employeeName: string;
  travelDetails?: { 
    travelDate: string;
    day?: string;
    fromLocation?: string;
    toLocation?: string;
    placeVisited?: string;
    modeOfTransport?: string;
    totalKm?: number;
    ratePerKm?: number;
    calculatedAmount?: number;
  };
  foodDetails?: { foodDate?: string; amount: number };
  hotelDetails?: { checkInDate: string; checkOutDate: string; amount: number };
  rechargeDetails?: { amount: number };
}

export default function Claims() {
  const { user } = useAuth();
  const [claims, setClaims] = useState<Claim[]>([]);
  const [categories, setCategories] = useState<{id: number, name: string, code: string, maxLimit?: number}[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState('ALL');
  const [employeeFilter, setEmployeeFilter] = useState('');
  
  // Modal States
  const [isSubmitModalOpen, setIsSubmitModalOpen] = useState(false);
  const [selectedClaimId, setSelectedClaimId] = useState<number | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadProgress, setUploadProgress] = useState<number | null>(null);
  
  // Zip Download Modal State
  const getDefaultDates = () => {
    const now = new Date();
    const past = new Date();
    past.setMonth(past.getMonth() - 1);
    return {
      start: past.toISOString().split('T')[0],
      end: now.toISOString().split('T')[0]
    };
  };

  const [isZipModalOpen, setIsZipModalOpen] = useState(false);
  const [zipDateRange, setZipDateRange] = useState(getDefaultDates());
  const [downloadingZip, setDownloadingZip] = useState(false);

  // Form State
  const [categoryId, setCategoryId] = useState('');
  const [selectedCategoryCode, setSelectedCategoryCode] = useState('');
  
  // Polymorphic Form Data
  const [travelDetails, setTravelDetails] = useState({ 
    travelDate: '', 
    day: '', 
    fromLocation: '', 
    toLocation: '', 
    placeVisited: '',
    modeOfTransport: 'Bus',
    totalKm: '', 
    ratePerKm: '' 
  });
  const [hotelDetails, setHotelDetails] = useState({ checkInDate: '', checkOutDate: '', amount: '' });
  const [foodDetails, setFoodDetails] = useState({ foodDate: '', amount: '' });
  const [rechargeDetails, setRechargeDetails] = useState({ amount: '' });

  const fetchClaims = async () => {
    try {
      setLoading(true);
      const endpoint = (user?.role === 'SUPER_ADMIN' || user?.role === 'ADMIN') ? '/claims/all' : '/claims/my';
      const res = await api.get(endpoint); 
      if (res.data.success) {
        setClaims(res.data.data);
      }
    } catch (err) {
      console.error('Failed to load claims', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchCategories = async () => {
    try {
      const res = await api.get('/expense-categories');
      if (res.data.success) {
        setCategories(res.data.data);
      }
    } catch (err) {
      console.error('Failed to load categories');
    }
  };

  useEffect(() => {
    fetchClaims();
    fetchCategories();
  }, []);

  const handleCategoryChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const id = e.target.value;
    setCategoryId(id);
    const cat = categories.find(c => c.id.toString() === id);
    setSelectedCategoryCode(cat?.code || '');
    
    // Automatically set the rate per km based on the admin setting
    if (cat?.code === 'TRAVEL') {
      setTravelDetails(prev => ({
        ...prev,
        ratePerKm: cat.maxLimit ? cat.maxLimit.toString() : '0'
      }));
    }
  };

  const handleTravelDateChange = (dateStr: string) => {
    if (!dateStr) {
      setTravelDetails(prev => ({ ...prev, travelDate: '', day: '' }));
      return;
    }
    const [y, m, d] = dateStr.split('-').map(Number);
    const dateObj = new Date(y, m - 1, d);
    const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const dayName = dayNames[dateObj.getDay()] || '';
    setTravelDetails(prev => ({
      ...prev,
      travelDate: dateStr,
      day: dayName
    }));
  };

  const handleCreate = async (e: React.FormEvent, isSubmit: boolean) => {
    e.preventDefault();
    try {
      const payload: any = {
        expenseCategoryId: parseInt(categoryId),
        isSubmit: (isSubmit && selectedFile) ? false : isSubmit
      };

      if (selectedCategoryCode === 'TRAVEL') {
        payload.travelDetails = {
          ...travelDetails,
          totalKm: parseFloat(travelDetails.totalKm),
          ratePerKm: parseFloat(travelDetails.ratePerKm)
        };
      } else if (selectedCategoryCode === 'HOTEL') {
        payload.hotelDetails = {
          ...hotelDetails,
          amount: parseFloat(hotelDetails.amount)
        };
      } else if (selectedCategoryCode === 'FOOD') {
        payload.foodDetails = { 
          foodDate: foodDetails.foodDate || null,
          amount: parseFloat(foodDetails.amount) 
        };
      } else if (selectedCategoryCode === 'RECHARGE') {
        payload.rechargeDetails = { amount: parseFloat(rechargeDetails.amount) };
      }

      const res = await api.post('/claims', payload);
      if (res.data.success) {
        const newClaimId = res.data.data.id;
        
        // Handle file upload if a file was selected
        if (selectedFile) {
          try {
            const formData = new FormData();
            formData.append('file', selectedFile);
            await api.post(`/attachments/claim/${newClaimId}`, formData, {
              headers: { 'Content-Type': 'multipart/form-data' },
              onUploadProgress: (progressEvent) => {
                const percentCompleted = Math.round((progressEvent.loaded * 100) / (progressEvent.total || 100));
                setUploadProgress(percentCompleted);
              }
            });

            // If the user originally intended to submit it, transition it from Draft to Submitted now
            if (isSubmit) {
              await api.post(`/claims/${newClaimId}/submit`);
            }
            alert(isSubmit ? 'Claim submitted and approved successfully with attachment!' : 'Claim saved as draft with attachment.');
          } catch (uploadErr) {
            console.error('File upload failed', uploadErr);
            alert('Claim created, but file upload failed.');
          } finally {
            setUploadProgress(null);
          }
        } else {
          alert(isSubmit ? 'Claim submitted and approved successfully!' : 'Claim saved as draft.');
        }

        setIsSubmitModalOpen(false);
        // Reset states
        setCategoryId('');
        setSelectedCategoryCode('');
        setTravelDetails({ 
          travelDate: '', 
          day: '', 
          fromLocation: '', 
          toLocation: '', 
          placeVisited: '',
          modeOfTransport: 'Bus',
          totalKm: '', 
          ratePerKm: '' 
        });
        setHotelDetails({ checkInDate: '', checkOutDate: '', amount: '' });
        setFoodDetails({ foodDate: '', amount: '' });
        setRechargeDetails({ amount: '' });
        setSelectedFile(null);
        fetchClaims();
      } else {
        alert(res.data.message || 'Failed to save claim');
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'An error occurred while saving the claim');
    }
  };

  const getStatusBadgeClass = (statusName: string) => {
    switch(statusName) {
      case 'Draft': return 'badge-blue';
      case 'Approved': return 'badge-green';
      case 'Rejected': return 'badge-red';
      default: return 'badge-blue'; // Submitted / Returned
    }
  };

  const handleExportExcel = async () => {
    try {
      const res = await api.get('/reports/claims/export', { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'ClaimsReport.xlsx');
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      console.error('Export failed', err);
      alert('Failed to export report');
    }
  };

  // Commented out approval flow temporarily as requested: all claims default to Approved
  /*
  const handleApproveAll = async () => {
    const pendingClaims = claims.filter(c => c.statusName === 'Submitted');
    if (pendingClaims.length === 0) {
      alert('There are no pending claims to approve.');
      return;
    }

    if (window.confirm(`Are you sure you want to approve all ${pendingClaims.length} pending claims?`)) {
      try {
        setLoading(true);
        const res = await api.post('/claims/approve-all', {});
        if (res.data.success) {
          alert(res.data.message || `Successfully approved ${pendingClaims.length} claims!`);
          await fetchClaims();
        } else {
          alert(res.data.message || 'Failed to approve claims');
        }
      } catch (err: any) {
        alert(err.response?.data?.message || 'An error occurred while approving claims');
      } finally {
        setLoading(false);
      }
    }
  };
  */

  const handleDownloadMonthlyZip = async () => {
    try {
      setDownloadingZip(true);
      const startIso = new Date(zipDateRange.start).toISOString();
      const endIso = new Date(zipDateRange.end + 'T23:59:59.999Z').toISOString();
      const res = await api.get(`/attachments/download-monthly-zip?startDate=${encodeURIComponent(startIso)}&endDate=${encodeURIComponent(endIso)}`, {
        responseType: 'blob'
      });

      const blob = new Blob([res.data], { type: 'application/zip' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Bills_${zipDateRange.start}_to_${zipDateRange.end}.zip`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      setIsZipModalOpen(false);
    } catch (err) {
      console.error('Failed to download bills zip', err);
      alert('Failed to download bills archive. Please check if any approved bills exist in that date range.');
    } finally {
      setDownloadingZip(false);
    }
  };

  const getDisplayDate = (claim: Claim) => {
    if (claim.claimDate) return claim.claimDate;
    if (claim.travelDetails?.travelDate) return claim.travelDetails.travelDate;
    if (claim.foodDetails?.foodDate) return claim.foodDetails.foodDate;
    if (claim.hotelDetails?.checkInDate) return claim.hotelDetails.checkInDate;
    if (claim.submittedAt) return new Date(claim.submittedAt).toLocaleDateString();
    return 'N/A';
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h1 style={{ fontSize: '1.875rem', fontWeight: 'bold', color: 'var(--text-main)' }}>Claims</h1>
          <p style={{ color: 'var(--text-muted)' }}>Manage and submit your expense claims.</p>
        </div>
        
        <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center', flexWrap: 'wrap' }}>
          {(user?.role === 'ADMIN' || user?.role === 'SUPER_ADMIN') && (
            <select 
              value={employeeFilter} 
              onChange={(e) => setEmployeeFilter(e.target.value)}
              style={{ padding: '0.5rem', borderRadius: '4px', border: '1px solid var(--border-color)', height: '40px' }}
            >
              <option value="">All Employees</option>
              {Array.from(new Set(claims.map(c => c.employeeName).filter(Boolean))).sort().map(emp => (
                <option key={emp} value={emp}>{emp}</option>
              ))}
            </select>
          )}
          <select 
            value={statusFilter} 
            onChange={(e) => setStatusFilter(e.target.value)}
            style={{ padding: '0.5rem', borderRadius: '4px', border: '1px solid var(--border-color)', height: '40px' }}
          >
            <option value="ALL">All Statuses</option>
            <option value="Approved">Approved</option>
            <option value="Draft">Draft</option>
            <option value="Submitted">Submitted (Pending)</option>
            <option value="Rejected">Rejected</option>
            <option value="Returned">Returned</option>
          </select>

          {(user?.role === 'ADMIN' || user?.role === 'SUPER_ADMIN') && (
            <>
              {/* Temporary: Approval flow disabled - claims are auto-approved */}
              {/*
              <button 
                className="btn-primary" 
                style={{ width: 'auto', background: 'var(--success, #10b981)', borderColor: 'var(--success, #10b981)', color: '#ffffff' }} 
                onClick={handleApproveAll}
                title="Approve all currently submitted pending claims"
              >
                ✓ Approve All
              </button>
              */}
              <button 
                className="btn-secondary" 
                style={{ width: 'auto' }} 
                onClick={() => {
                  setZipDateRange(getDefaultDates());
                  setIsZipModalOpen(true);
                }}
                title="Download approved monthly receipts as employee-structured ZIP"
              >
                📦 Download Bills (ZIP)
              </button>
              <button className="btn-secondary" style={{ width: 'auto' }} onClick={handleExportExcel}>
                ⬇ Export Excel
              </button>
            </>
          )}
          <button className="btn-primary" style={{ width: 'auto' }} onClick={() => setIsSubmitModalOpen(true)}>
            + Submit Claim
          </button>
        </div>
      </div>

      <div className="table-container">
        {loading ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-muted)' }}>Loading...</div>
        ) : (
          <table className="premium-table">
            <thead>
              <tr>
                <th>Claim No.</th>
                <th>Category</th>
                {user?.role !== 'EMPLOYEE' && <th>Employee</th>}
                <th>Amount</th>
                <th>Status</th>
                <th>Date</th>
              </tr>
            </thead>
            <tbody>
              {claims.filter(c => 
                (statusFilter === 'ALL' || c.statusName === statusFilter) && 
                (!employeeFilter || c.employeeName === employeeFilter)
              ).length === 0 ? (
                <tr>
                  <td colSpan={user?.role !== 'EMPLOYEE' ? 6 : 5} style={{ textAlign: 'center', color: 'var(--text-muted)' }}>No claims found.</td>
                </tr>
              ) : (
                claims.filter(c => 
                  (statusFilter === 'ALL' || c.statusName === statusFilter) && 
                  (!employeeFilter || c.employeeName === employeeFilter)
                ).map(claim => (
                  <tr key={claim.id} onClick={() => setSelectedClaimId(claim.id)} style={{ cursor: 'pointer' }}>
                    <td style={{ fontWeight: '600', color: 'var(--primary-color)' }}>{claim.claimNumber}</td>
                    <td>{claim.expenseCategoryName}</td>
                    {user?.role !== 'EMPLOYEE' && <td>{claim.employeeName}</td>}
                    <td style={{ fontWeight: '600' }}>₹{(claim.totalAmount || 0).toFixed(2)}</td>
                    <td>
                      <span className={`badge ${getStatusBadgeClass(claim.statusName)}`}>
                        {claim.statusName}
                      </span>
                    </td>
                    <td>{getDisplayDate(claim)}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        )}
      </div>

      {selectedClaimId && (
        <ClaimDetailsModal 
          claimId={selectedClaimId} 
          onClose={() => setSelectedClaimId(null)}
          onRefresh={fetchClaims}
        />
      )}

      {/* Polymorphic Submit Modal */}
      {isSubmitModalOpen && (
        <div className="modal-overlay" onClick={() => setIsSubmitModalOpen(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()} style={{ maxWidth: '620px', maxHeight: '90vh', overflowY: 'auto' }}>
            <h2 style={{ marginBottom: '1.5rem', fontSize: '1.5rem' }}>New Expense Claim</h2>
            
            <form onSubmit={(e) => handleCreate(e, true)}>
              <div className="form-group">
                <label>Expense Category</label>
                <select required value={categoryId} onChange={handleCategoryChange}>
                  <option value="">Select Category...</option>
                  {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>

              {selectedCategoryCode === 'TRAVEL' && (
                <>
                  <div className="form-group-grid">
                    <div className="form-group">
                      <label>Travel Date</label>
                      <input 
                        type="date" 
                        required 
                        value={travelDetails.travelDate} 
                        onChange={e => handleTravelDateChange(e.target.value)} 
                      />
                    </div>
                    <div className="form-group">
                      <label>Day (Auto-Selected)</label>
                      <input 
                        type="text" 
                        required 
                        readOnly 
                        value={travelDetails.day} 
                        placeholder="Select a date first"
                        style={{ backgroundColor: '#f1f5f9', cursor: 'not-allowed' }}
                      />
                    </div>
                  </div>
                  <div className="form-group-grid">
                    <div className="form-group">
                      <label>Start Point (From)</label>
                      <input 
                        required 
                        value={travelDetails.fromLocation} 
                        onChange={e => setTravelDetails({...travelDetails, fromLocation: e.target.value})} 
                        placeholder="e.g. Ahmedabad"
                      />
                    </div>
                    <div className="form-group">
                      <label>End Point (To)</label>
                      <input 
                        required 
                        value={travelDetails.toLocation} 
                        onChange={e => setTravelDetails({...travelDetails, toLocation: e.target.value})} 
                        placeholder="e.g. Gandhinagar"
                      />
                    </div>
                  </div>
                  <div className="form-group-grid">
                    <div className="form-group">
                      <label>Place Visited</label>
                      <input 
                        value={travelDetails.placeVisited} 
                        onChange={e => setTravelDetails({...travelDetails, placeVisited: e.target.value})} 
                        placeholder="e.g. Old Counter Visit, Market Visit"
                      />
                    </div>
                    <div className="form-group">
                      <label>Mode of Transport</label>
                      <select 
                        value={travelDetails.modeOfTransport} 
                        onChange={e => setTravelDetails({...travelDetails, modeOfTransport: e.target.value})}
                      >
                        <option value="Bus">Bus</option>
                        <option value="Train">Train</option>
                        <option value="Auto">Auto</option>
                        <option value="Cab">Cab</option>
                        <option value="Bike / Personal Vehicle">Bike / Personal Vehicle</option>
                      </select>
                    </div>
                  </div>
                  <div className="form-group-grid">
                    <div className="form-group">
                      <label>Total Km</label>
                      <input type="number" step="0.1" required value={travelDetails.totalKm} onChange={e => setTravelDetails({...travelDetails, totalKm: e.target.value})} />
                    </div>
                    <div className="form-group">
                      <label>Rate Per Km</label>
                      <input 
                        type="number" 
                        step="0.1" 
                        required 
                        readOnly 
                        value={travelDetails.ratePerKm} 
                        title="Rate is set by the admin"
                        style={{ backgroundColor: '#e2e8f0', cursor: 'not-allowed' }}
                      />
                    </div>
                  </div>
                </>
              )}

              {selectedCategoryCode === 'HOTEL' && (
                <>
                  <div className="form-group-grid">
                    <div className="form-group">
                      <label>Check-in Date</label>
                      <input type="date" required value={hotelDetails.checkInDate} onChange={e => setHotelDetails({...hotelDetails, checkInDate: e.target.value})} />
                    </div>
                    <div className="form-group">
                      <label>Check-out Date</label>
                      <input type="date" required value={hotelDetails.checkOutDate} onChange={e => setHotelDetails({...hotelDetails, checkOutDate: e.target.value})} />
                    </div>
                  </div>
                  <div className="form-group">
                    <label>Total Amount (Accommodation)</label>
                    <input type="number" step="0.01" required value={hotelDetails.amount} onChange={e => setHotelDetails({...hotelDetails, amount: e.target.value})} />
                  </div>
                </>
              )}

              {selectedCategoryCode === 'FOOD' && (
                <>
                  <div className="form-group">
                    <label>Food / Meal Date</label>
                    <input 
                      type="date" 
                      required 
                      value={foodDetails.foodDate} 
                      onChange={e => setFoodDetails({...foodDetails, foodDate: e.target.value})} 
                    />
                  </div>
                  <div className="form-group">
                    <label>Daily Allowance Amount</label>
                    <input 
                      type="number" 
                      step="0.01" 
                      required 
                      value={foodDetails.amount} 
                      onChange={e => setFoodDetails({...foodDetails, amount: e.target.value})} 
                    />
                  </div>
                </>
              )}

              {selectedCategoryCode === 'RECHARGE' && (
                <div className="form-group">
                  <label>Mobile Recharge Amount</label>
                  <input 
                    type="number" 
                    step="0.01" 
                    required 
                    value={rechargeDetails.amount} 
                    onChange={e => setRechargeDetails({ amount: e.target.value })} 
                  />
                </div>
              )}

              {selectedCategoryCode !== '' && (
                <div className="form-group">
                  <label>Attachment (Receipt)</label>
                  <input 
                    type="file" 
                    accept=".jpg,.jpeg,.png,.pdf" 
                    onChange={e => setSelectedFile(e.target.files?.[0] || null)} 
                    disabled={uploadProgress !== null}
                  />
                  {uploadProgress !== null && (
                    <div style={{ marginTop: '1rem' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.25rem', fontSize: '0.875rem', fontWeight: '500' }}>
                        <span>Uploading...</span>
                        <span>{uploadProgress}%</span>
                      </div>
                      <div style={{ width: '100%', height: '8px', background: '#e2e8f0', borderRadius: '4px', overflow: 'hidden' }}>
                        <div style={{ width: `${uploadProgress}%`, height: '100%', background: 'var(--primary-color)', transition: 'width 0.2s ease-in-out' }} />
                      </div>
                    </div>
                  )}
                </div>
              )}

              <div className="form-group-grid" style={{ marginTop: '2rem' }}>
                <button type="button" className="btn-secondary" onClick={(e) => handleCreate(e, false)} disabled={uploadProgress !== null}>
                  Save Draft
                </button>
                <button type="submit" className="btn-primary" disabled={uploadProgress !== null}>
                  Submit Claim
                </button>
              </div>
              <button type="button" onClick={() => setIsSubmitModalOpen(false)} style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', marginTop: '1rem', width: '100%', cursor: 'pointer' }}>
                Cancel
              </button>
            </form>
          </div>
        </div>
      )}

      {/* Monthly Bills ZIP Download Modal */}
      {isZipModalOpen && (
        <div className="modal-overlay" onClick={() => setIsZipModalOpen(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()} style={{ maxWidth: '480px' }}>
            <h2 style={{ marginBottom: '0.75rem', fontSize: '1.35rem' }}>Download Bills Archive (ZIP)</h2>
            <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', fontSize: '0.875rem', lineHeight: '1.5' }}>
              Downloads a structured ZIP archive containing all uploaded JPGs and PDFs of approved claims organized into folders by employee for the specified date range.
            </p>

            <div className="form-group-grid">
              <div className="form-group">
                <label>From Date</label>
                <input 
                  type="date" 
                  value={zipDateRange.start} 
                  onChange={e => setZipDateRange({ ...zipDateRange, start: e.target.value })} 
                />
              </div>
              <div className="form-group">
                <label>To Date</label>
                <input 
                  type="date" 
                  value={zipDateRange.end} 
                  onChange={e => setZipDateRange({ ...zipDateRange, end: e.target.value })} 
                />
              </div>
            </div>

            <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem' }}>
              <button 
                type="button" 
                className="btn-primary" 
                onClick={handleDownloadMonthlyZip} 
                disabled={downloadingZip}
                style={{ flex: 1 }}
              >
                {downloadingZip ? 'Generating ZIP...' : '⬇ Download ZIP'}
              </button>
              <button 
                type="button" 
                className="btn-secondary" 
                onClick={() => setIsZipModalOpen(false)}
                disabled={downloadingZip}
                style={{ width: 'auto' }}
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
