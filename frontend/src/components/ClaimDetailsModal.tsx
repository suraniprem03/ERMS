import { useState, useEffect } from 'react';
import api from '../utils/api';
import { useAuth } from '../contexts/AuthContext';

interface ClaimDetailsModalProps {
  claimId: number;
  onClose: () => void;
  onRefresh?: () => void;
}

export default function ClaimDetailsModal({ claimId, onClose, onRefresh }: ClaimDetailsModalProps) {
  const { user } = useAuth();
  const [claim, setClaim] = useState<any>(null);
  const [attachments, setAttachments] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  
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
        if (onRefresh) onRefresh();
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

  const totalAmount = (claim.totalAmount !== undefined && claim.totalAmount !== null)
    ? claim.totalAmount
    : (claim.travelDetails?.calculatedAmount || claim.hotelDetails?.amount || claim.foodDetails?.amount || claim.rechargeDetails?.amount || 0);

  const displayDate = claim.claimDate 
    || claim.travelDetails?.travelDate 
    || claim.foodDetails?.foodDate 
    || claim.hotelDetails?.checkInDate 
    || (claim.submittedAt ? new Date(claim.submittedAt).toLocaleDateString() : 'N/A');

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()} style={{ maxWidth: '700px', maxHeight: '90vh', overflowY: 'auto' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
          <h2 style={{ margin: 0 }}>Claim Details: {claim.claimNumber}</h2>
          <button onClick={onClose} style={{ background: 'none', border: 'none', fontSize: '1.5rem', cursor: 'pointer', color: 'var(--text-muted)' }}>×</button>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginBottom: '1.5rem' }}>
          <div><strong>Employee:</strong> {claim.employeeName}</div>
          <div><strong>Category:</strong> {claim.expenseCategoryName}</div>
          <div><strong>Amount:</strong> ₹{Number(totalAmount).toFixed(2)}</div>
          <div><strong>Status:</strong> {claim.statusName}</div>
          <div><strong>Expense Date:</strong> {displayDate}</div>
          {claim.submittedAt && <div><strong>Submitted At:</strong> {new Date(claim.submittedAt).toLocaleDateString()}</div>}
        </div>

        {/* Polymorphic Details */}
        <div className="glass-panel" style={{ padding: '1rem', marginBottom: '1.5rem', background: '#f8fafc' }}>
          <h3 style={{ marginBottom: '1rem', fontSize: '1rem' }}>Expense Specifics</h3>
          {claim.travelDetails && (
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.5rem', fontSize: '0.875rem' }}>
              <div><strong>Travel Date:</strong> {claim.travelDetails.travelDate}</div>
              <div><strong>Day:</strong> {claim.travelDetails.day}</div>
              <div><strong>Start Point:</strong> {claim.travelDetails.fromLocation}</div>
              <div><strong>End Point:</strong> {claim.travelDetails.toLocation}</div>
              {claim.travelDetails.placeVisited && <div><strong>Place Visited:</strong> {claim.travelDetails.placeVisited}</div>}
              {claim.travelDetails.modeOfTransport && <div><strong>Transport Mode:</strong> {claim.travelDetails.modeOfTransport}</div>}
              <div><strong>Total Km:</strong> {claim.travelDetails.totalKm} km</div>
              <div><strong>Rate/Km:</strong> ₹{claim.travelDetails.ratePerKm}</div>
              <div><strong>Calculated Amount:</strong> ₹{claim.travelDetails.calculatedAmount}</div>
            </div>
          )}
          {claim.hotelDetails && (
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.5rem', fontSize: '0.875rem' }}>
              <div><strong>Check-in:</strong> {claim.hotelDetails.checkInDate}</div>
              <div><strong>Check-out:</strong> {claim.hotelDetails.checkOutDate}</div>
              <div><strong>Total Amount:</strong> ₹{claim.hotelDetails.amount}</div>
            </div>
          )}
          {claim.foodDetails && (
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.5rem', fontSize: '0.875rem' }}>
              <div><strong>Food Date:</strong> {claim.foodDetails.foodDate || 'N/A'}</div>
              <div><strong>Daily Allowance Amount:</strong> ₹{claim.foodDetails.amount}</div>
            </div>
          )}
          {claim.rechargeDetails && (
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.5rem', fontSize: '0.875rem' }}>
              <div><strong>Mobile Recharge Amount:</strong> ₹{claim.rechargeDetails.amount}</div>
            </div>
          )}
          {!claim.travelDetails && !claim.hotelDetails && !claim.foodDetails && !claim.rechargeDetails && (
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

      </div>
    </div>
  );
}
