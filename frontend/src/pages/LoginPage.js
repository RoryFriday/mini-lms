import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { login, register } from '../api';

export default function LoginPage() {
  const [isRegister, setIsRegister] = useState(false);
  const [form, setForm] = useState({ email: '', password: '', firstName: '', lastName: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { loginUser } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const res = isRegister
        ? await register(form)
        : await login({ email: form.email, password: form.password });
      loginUser(res.data);
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.message || 'Something went wrong');
    } finally {
      setLoading(false);
    }
  };

  const update = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  return (
    <div className="auth-page">
      <form className="auth-form" onSubmit={handleSubmit}>
        <h2>{isRegister ? 'Create Account' : 'Sign In'}</h2>
        {error && <div className="alert alert-error">{error}</div>}

        {isRegister && (
          <>
            <div className="form-group">
              <label>First Name</label>
              <input value={form.firstName} onChange={update('firstName')} required />
            </div>
            <div className="form-group">
              <label>Last Name</label>
              <input value={form.lastName} onChange={update('lastName')} required />
            </div>
          </>
        )}

        <div className="form-group">
          <label>Email</label>
          <input type="email" value={form.email} onChange={update('email')} required />
        </div>
        <div className="form-group">
          <label>Password</label>
          <input type="password" value={form.password} onChange={update('password')} required />
        </div>

        <button className="btn btn-primary btn-block" disabled={loading}>
          {loading ? 'Please wait...' : (isRegister ? 'Register' : 'Sign In')}
        </button>

        <div className="toggle">
          <button type="button" onClick={() => { setIsRegister(!isRegister); setError(''); }}>
            {isRegister ? 'Already have an account? Sign in' : "Don't have an account? Register"}
          </button>
        </div>

        {!isRegister && (
          <div style={{ marginTop: 16, padding: 12, background: '#f7fafc', borderRadius: 8, fontSize: '0.85rem' }}>
            <strong>Demo accounts:</strong><br />
            Admin: admin@library.com / Admin123!<br />
            Librarian: librarian@library.com / Librarian123!
          </div>
        )}
      </form>
    </div>
  );
}
