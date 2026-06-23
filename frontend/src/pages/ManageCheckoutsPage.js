import React, { useState, useEffect } from 'react';
import { getAllCheckouts, returnBookOnBehalf } from '../api';

export default function ManageCheckoutsPage() {
  const [checkouts, setCheckouts] = useState([]);
  const [activeOnly, setActiveOnly] = useState(true);
  const [message, setMessage] = useState('');

  const fetchCheckouts = async () => {
    try {
      const res = await getAllCheckouts(activeOnly);
      setCheckouts(res.data);
    } catch (err) {
      console.error('Failed to fetch checkouts', err);
    }
  };

  useEffect(() => { fetchCheckouts(); }, [activeOnly]);

  const handleReturn = async (id) => {
    try {
      await returnBookOnBehalf(id);
      setMessage('Book returned successfully!');
      fetchCheckouts();
      setTimeout(() => setMessage(''), 3000);
    } catch (err) {
      setMessage(err.response?.data?.message || 'Return failed');
      setTimeout(() => setMessage(''), 3000);
    }
  };

  const isOverdue = (dueDate, returnedAt) => !returnedAt && new Date(dueDate) < new Date();

  return (
    <div className="page">
      <div className="page-header">
        <h2>📊 All Checkouts</h2>
        <label style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <input type="checkbox" checked={activeOnly} onChange={(e) => setActiveOnly(e.target.checked)} />
          Active only
        </label>
      </div>

      {message && <div className={`alert ${message.includes('success') ? 'alert-success' : 'alert-error'}`}>{message}</div>}

      {checkouts.length === 0 ? (
        <p style={{ textAlign: 'center', color: '#718096', marginTop: 40 }}>No checkouts found.</p>
      ) : (
        <table className="checkout-table">
          <thead>
            <tr>
              <th>Book</th>
              <th>Author</th>
              <th>User</th>
              <th>Checked Out</th>
              <th>Due Date</th>
              <th>Status</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {checkouts.map(c => (
              <tr key={c.id}>
                <td>{c.bookTitle}</td>
                <td>{c.bookAuthor}</td>
                <td>{c.userEmail}</td>
                <td>{new Date(c.checkedOutAt).toLocaleDateString()}</td>
                <td className={isOverdue(c.dueDate, c.returnedAt) ? 'overdue' : ''}>
                  {new Date(c.dueDate).toLocaleDateString()}
                  {isOverdue(c.dueDate, c.returnedAt) && ' (OVERDUE)'}
                </td>
                <td>
                  {c.returnedAt ? (
                    <span className="returned">Returned</span>
                  ) : (
                    <span style={{ color: '#dd6b20' }}>Active</span>
                  )}
                </td>
                <td>
                  {!c.returnedAt && (
                    <button className="btn btn-success btn-sm" onClick={() => handleReturn(c.id)}>
                      Return
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
