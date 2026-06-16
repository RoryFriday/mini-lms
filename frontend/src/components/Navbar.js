import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { user, logout, isLibrarian, isAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="navbar">
      <h1>📚 Library Management System</h1>
      <nav>
        <Link to="/">Books</Link>
        <Link to="/my-checkouts">My Checkouts</Link>
        {isLibrarian && <Link to="/checkouts">Manage Checkouts</Link>}
        {isAdmin && <Link to="/users">Users</Link>}
        <span className="user-info">{user?.firstName} ({user?.role})</span>
        <button onClick={handleLogout}>Logout</button>
      </nav>
    </div>
  );
}
