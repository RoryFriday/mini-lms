import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import Navbar from './components/Navbar';
import LoginPage from './pages/LoginPage';
import BooksPage from './pages/BooksPage';
import MyCheckoutsPage from './pages/MyCheckoutsPage';
import ManageCheckoutsPage from './pages/ManageCheckoutsPage';
import UsersPage from './pages/UsersPage';

function ProtectedRoute({ children, roles }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="container page">Loading...</div>;
  if (!user) return <Navigate to="/login" />;
  if (roles && !roles.includes(user.role)) return <Navigate to="/" />;
  return children;
}

export default function App() {
  const { user, loading } = useAuth();

  if (loading) return <div className="container page">Loading...</div>;

  return (
    <>
      {user && <Navbar />}
      <div className="container">
        <Routes>
          <Route path="/login" element={user ? <Navigate to="/" /> : <LoginPage />} />
          <Route path="/" element={
            <ProtectedRoute><BooksPage /></ProtectedRoute>
          } />
          <Route path="/my-checkouts" element={
            <ProtectedRoute><MyCheckoutsPage /></ProtectedRoute>
          } />
          <Route path="/checkouts" element={
            <ProtectedRoute roles={['Librarian', 'Admin']}><ManageCheckoutsPage /></ProtectedRoute>
          } />
          <Route path="/users" element={
            <ProtectedRoute roles={['Admin']}><UsersPage /></ProtectedRoute>
          } />
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </div>
    </>
  );
}
