import React, { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../context/AuthContext';
import { searchBooks, createBook, updateBook, deleteBook, checkoutBook } from '../api';

function BookModal({ book, onClose, onSave }) {
  const [form, setForm] = useState(book || {
    title: '', author: '', isbn: '', genre: '', description: '',
    publicationYear: new Date().getFullYear(), publisher: '', totalCopies: 1
  });
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      if (book) {
        await updateBook(book.id, form);
      } else {
        await createBook(form);
      }
      onSave();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save book');
    } finally {
      setSaving(false);
    }
  };

  const update = (field) => (e) => setForm({ ...form, [field]: e.target.value });
  const updateNum = (field) => (e) => setForm({ ...form, [field]: parseInt(e.target.value) || 0 });

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" onClick={e => e.stopPropagation()}>
        <h2>{book ? 'Edit Book' : 'Add New Book'}</h2>
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Title *</label>
            <input value={form.title} onChange={update('title')} required />
          </div>
          <div className="form-group">
            <label>Author *</label>
            <input value={form.author} onChange={update('author')} required />
          </div>
          <div className="form-group">
            <label>ISBN *</label>
            <input value={form.isbn} onChange={update('isbn')} required />
          </div>
          <div className="form-group">
            <label>Genre</label>
            <input value={form.genre} onChange={update('genre')} />
          </div>
          <div className="form-group">
            <label>Description</label>
            <textarea value={form.description} onChange={update('description')} rows={3} />
          </div>
          <div className="form-group">
            <label>Publication Year</label>
            <input type="number" value={form.publicationYear} onChange={updateNum('publicationYear')} />
          </div>
          <div className="form-group">
            <label>Publisher</label>
            <input value={form.publisher} onChange={update('publisher')} />
          </div>
          <div className="form-group">
            <label>Total Copies</label>
            <input type="number" min="1" value={form.totalCopies} onChange={updateNum('totalCopies')} />
          </div>
          <div className="modal-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Saving...' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default function BooksPage() {
  const { user, isLibrarian, isAdmin } = useAuth();
  const [books, setBooks] = useState([]);
  const [query, setQuery] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [editBook, setEditBook] = useState(null);
  const [message, setMessage] = useState('');

  const fetchBooks = useCallback(async () => {
    try {
      const res = await searchBooks({ query, page, pageSize: 12 });
      setBooks(res.data.items);
      setTotalPages(res.data.totalPages);
    } catch (err) {
      console.error('Failed to fetch books', err);
    }
  }, [query, page]);

  useEffect(() => { fetchBooks(); }, [fetchBooks]);

  const handleCheckout = async (bookId) => {
    try {
      await checkoutBook(bookId);
      setMessage('Book checked out successfully!');
      fetchBooks();
      setTimeout(() => setMessage(''), 3000);
    } catch (err) {
      setMessage(err.response?.data?.message || 'Checkout failed');
      setTimeout(() => setMessage(''), 3000);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this book?')) return;
    try {
      await deleteBook(id);
      fetchBooks();
    } catch (err) {
      setMessage('Failed to delete book');
      setTimeout(() => setMessage(''), 3000);
    }
  };

  const handleSave = () => {
    setShowModal(false);
    setEditBook(null);
    fetchBooks();
  };

  return (
    <div className="page">
      <div className="page-header">
        <h2>📖 Book Catalog</h2>
        {isLibrarian && (
          <button className="btn btn-primary" onClick={() => { setEditBook(null); setShowModal(true); }}>
            + Add Book
          </button>
        )}
      </div>

      {message && <div className={`alert ${message.includes('success') ? 'alert-success' : 'alert-error'}`}>{message}</div>}

      <div className="search-bar">
        <input
          placeholder="Search books by title, author, ISBN, genre..."
          value={query}
          onChange={(e) => { setQuery(e.target.value); setPage(1); }}
        />
      </div>

      <div className="book-grid">
        {books.map(book => (
          <div key={book.id} className="book-card">
            <h3>{book.title}</h3>
            <div className="author">by {book.author}</div>
            <div className="meta">ISBN: {book.isbn} · {book.publicationYear} · {book.publisher}</div>
            <div className="meta">Genre: {book.genre}</div>
            <div className="description">{book.description}</div>
            <span className={`availability ${book.availableCopies > 0 ? 'available' : 'unavailable'}`}>
              {book.availableCopies > 0
                ? `${book.availableCopies} of ${book.totalCopies} available`
                : 'All copies checked out'}
            </span>
            <div className="actions">
              {user && book.availableCopies > 0 && (
                <button className="btn btn-success btn-sm" onClick={() => handleCheckout(book.id)}>
                  Checkout
                </button>
              )}
              {isLibrarian && (
                <button className="btn btn-primary btn-sm" onClick={() => { setEditBook(book); setShowModal(true); }}>
                  Edit
                </button>
              )}
              {isAdmin && (
                <button className="btn btn-danger btn-sm" onClick={() => handleDelete(book.id)}>
                  Delete
                </button>
              )}
            </div>
          </div>
        ))}
      </div>

      {books.length === 0 && <p style={{ textAlign: 'center', color: '#718096', marginTop: 40 }}>No books found.</p>}

      {totalPages > 1 && (
        <div className="pagination">
          <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}>← Prev</button>
          {Array.from({ length: totalPages }, (_, i) => (
            <button key={i + 1} className={page === i + 1 ? 'active' : ''} onClick={() => setPage(i + 1)}>
              {i + 1}
            </button>
          ))}
          <button disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next →</button>
        </div>
      )}

      {showModal && <BookModal book={editBook} onClose={() => { setShowModal(false); setEditBook(null); }} onSave={handleSave} />}
    </div>
  );
}
