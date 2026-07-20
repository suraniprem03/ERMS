import { useState, useEffect } from 'react';
import api from '../utils/api';
import { useAuth } from '../contexts/AuthContext';

interface ClaimDetailsModalProps {
  claimId: number;
  onClose: () => void;
  onRefresh: () => void;
}

export default function ClaimDetailsModal({ claimId, onClose, onRefresh }: ClaimDetailsModalProps) {
  const { user } = useAuth();
  const [claim, setClaim] = useState<any>(null);
  const [attachments, setAttachments] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  
  const [remarks, setRemarks] = useState('');
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);

  useEffect(() => {
    fetchClaimDetails();
    fetchAttachments();
  }, [claimId]);

  const fetchClaimDetails = async () => {
    try {
      const res = await api.get(`/claims/${claimId}`);
      if (res.data.success) {
        setClaim(res.data.data);
      }
    } catch (err) {
      console.error('Failed to fetch claim details', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchAttachments = async () => {
    if (claim?.attachments) {
      setAttachments(claim.attachments);
    }
  };

  useEffect(() => {
    if (claim) {
      fetchAttachments();
    }
  }, [claim]);

  const handleAdminAction = async (actionId: number) => {
    if (actionId === 5 && !remarks) {
      alert("Remarks are mandatory for rejection.");
      return;
    }
    
    try {
      const payload = {
        action: actionId, // 4 = Approved, 5 = Rejected, 3 = Returned
        remarks: remarks || null
      };
      const res = await api.post(`/claims/${claimId}/process`, payload);
      if (res.data.success) {
        onRefresh();
        onClose();
      } else {
        alert(res.data.message || 'Action failed');
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'An error occurred');
    }
  };

  const handleFileUpload = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file) return;

    try {
      setUploading(true);
      const formData = new FormData();
      formData.append('file', file);
      
      const res = await api.post(`/attachments/claim/${claimId}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      
      if (res.data.success) {
        setFile(null);
        fetchClaimDetails();
      } else {
        alert(res.data.message || 'Upload failed');
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Error uploading file');
    } finally {
      setUploading(false);
    }
  };

  const downloadAttachment = async (attachmentId: number, originalName: string) => {
    try {
      const res = await api.get(`/attachments/${attachmentId}/download`, { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', originalName);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      console.error('Download failed', err);
      alert('Failed to download file');
    }
  };

  if (loading) {
    return (
      <div className="modal-overlay">
        <div className="modal-content"><p>Loading details...</p></div>
      </div>
    );
  }

  if (!claim) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()} style={{ maxWidth: '700px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
          <h2 style={{ margin: 0 }}>Claim Details: {claim.claimNumber}</h2>
          <button onClick={onClose} style={{ background: 'none', border: 'none', fontSize: '1.5rem', cursor: 'pointer', color: 'var(--text-muted)' }}>×</button>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginBottom: '1.5rem' }}>
          <div><strong>Employee:</strong> {claim.employeeName}</div>
          <div><strong>Category:</strong> {claim.expenseCategoryName}</div>
          <div><strong>Amount:</strong> ₹{claim.travelDetails?.totalAmount || claim.hotelDetails?.amount || claim.foodDetails?.amount || claim.rechargeDetails?.amount || 0}</div>
          <div><strong>Status:</strong> {claim.statusName}</div>
        </div>

        {/* Polymorphic Details */}
        <div className="glass-panel" style={{ padding: '1rem', marginBottom: '1.5rem', background: '#f8fafc' }}>
          <h3 style={{ marginBottom: '1rem', fontSize: '1rem' }}>Expense Specifics</h3>
          {claim.travelDetails && (
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.5rem', fontSize: '0.875rem' }}>
              <div><strong>Date:</strong> {claim.travelDetails.travelDate}</div>
              <div><strong>Day:</strong> {claim.travelDetails.day}</div>
              <div><strong>From:</strong> {claim.travelDetails.fromLocation}</div>
              <div><strong>To:</strong> {claim.travelDetails.toLocation}</div>
              <div><strong>Total Km:</strong> {claim.travelDetails.totalKm} km</div>
              <div><strong>Rate/Km:</strong> ₹{claim.travelDetails.ratePerKm}</div>
            </div>
          )}
          {claim.hotelDetails && (
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.5rem', fontSize: '0.875rem' }}>
              <div><strong>Check-in:</strong> {claim.hotelDetails.checkInDate}</div>
              <div><strong>Check-out:</strong> {claim.hotelDetails.checkOutDate}</div>
            </div>
          )}
          {!claim.travelDetails && !claim.hotelDetails && (
             <p style={{ fontSize: '0.875rem' }}>Standard flat rate expense.</p>
          )}
        </div>

        {/* Attachments */}
        <div style={{ marginBottom: '2rem' }}>
          <h3 style={{ marginBottom: '1rem', fontSize: '1rem' }}>Attachments (Receipts)</h3>
          
          {attachments.length > 0 ? (
            <ul style={{ listStyleType: 'none', padding: 0, marginBottom: '1rem' }}>
              {attachments.map(att => (
                <li key={att.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.5rem', background: '#f1f5f9', borderRadius: '4px', marginBottom: '0.5rem' }}>
                  <span style={{ fontSize: '0.875rem' }}>{att.originalFileName}</span>
                  <button onClick={() => downloadAttachment(att.id, att.originalFileName)} style={{ background: 'none', border: 'none', color: 'var(--primary-color)', cursor: 'pointer', fontSize: '0.875rem', fontWeight: 'bold' }}>Download</button>
                </li>
              ))}
            </ul>
          ) : (
            <p style={{ fontSize: '0.875rem', color: 'var(--text-muted)' }}>No attachments found.</p>
          )}

          {user?.role === 'EMPLOYEE' && claim.statusName === 'Draft' && (
            <form onSubmit={handleFileUpload} style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
              <input type="file" onChange={e => setFile(e.target.files ? e.target.files[0] : null)} style={{ fontSize: '0.875rem' }} />
              <button type="submit" className="btn-secondary" disabled={!file || uploading} style={{ padding: '0.25rem 0.5rem', width: 'auto' }}>
                {uploading ? 'Uploading...' : 'Upload'}
              </button>
            </form>
          )}
        </div>

        {/* Admin Actions */}
        {(user?.role === 'ADMIN' || user?.role === 'SUPER_ADMIN') && claim.statusName === 'Submitted' && (
          <div style={{ borderTop: '1px solid var(--border-color)', paddingTop: '1.5rem' }}>
            <h3 style={{ marginBottom: '1rem', fontSize: '1rem' }}>Admin Actions</h3>
            <textarea 
              placeholder="Enter remarks (Mandatory for rejection)" 
              value={remarks} 
              onChange={e => setRemarks(e.target.value)}
              style={{ width: '100%', padding: '0.75rem', borderRadius: '8px', border: '1px solid var(--border-color)', marginBottom: '1rem', minHeight: '80px' }}
            />
            <div style={{ display: 'flex', gap: '1rem' }}>
              <button className="btn-primary" style={{ background: 'var(--success, #10b981)', borderColor: 'var(--success, #10b981)', color: '#ffffff' }} onClick={() => handleAdminAction(4)}>
                Approve Claim
              </button>
              <button className="btn-primary" style={{ background: 'var(--error, #ef4444)', borderColor: 'var(--error, #ef4444)', color: '#ffffff' }} onClick={() => handleAdminAction(5)}>
                Reject Claim
              </button>
            </div>
          </div>
        )}

      </div>
    </div>
  );
}
