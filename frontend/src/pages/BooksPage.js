import React, { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../context/AuthContext';
import { searchBooks, createBook, updateBook, deleteBook, checkoutBook, aiSearchBooks, aiSearchStatus } from '../api';

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

  // AI search state
  const [smartSearch, setSmartSearch] = useState(false);
  const [aiAvailable, setAiAvailable] = useState(false);
  const [aiResults, setAiResults] = useState(null); // { items, totalCount }
  const [aiLoading, setAiLoading] = useState(false);
  const [aiError, setAiError] = useState('');

  // Check if AI search is available on mount
  useEffect(() => {
    aiSearchStatus()
      .then(res => setAiAvailable(res.data.available))
      .catch(() => setAiAvailable(false));
  }, []);

  // Standard search
  const fetchBooks = useCallback(async () => {
    if (smartSearch) return; // don't run standard search in smart mode
    try {
      const res = await searchBooks({ query, page, pageSize: 12 });
      setBooks(res.data.items);
      setTotalPages(res.data.totalPages);
    } catch (err) {
      console.error('Failed to fetch books', err);
    }
  }, [query, page, smartSearch]);

  useEffect(() => { fetchBooks(); }, [fetchBooks]);

  // Clear AI results when toggling off or clearing query
  useEffect(() => {
    if (!smartSearch) {
      setAiResults(null);
      setAiError('');
    }
  }, [smartSearch]);

  // AI search handler
  const handleAiSearch = async () => {
    if (!query.trim()) return;
    setAiLoading(true);
    setAiError('');
    setAiResults(null);
    try {
      const res = await aiSearchBooks(query);
      setAiResults(res.data);
    } catch (err) {
      const msg = err.response?.data?.message || 'AI search failed';
      setAiError(msg);
      // Fallback: run standard search
      try {
        const fallback = await searchBooks({ query, page: 1, pageSize: 50 });
        setBooks(fallback.data.items);
        setTotalPages(fallback.data.totalPages);
      } catch { /* ignore */ }
    } finally {
      setAiLoading(false);
    }
  };

  // Trigger AI search on Enter key
  const handleKeyDown = (e) => {
    if (smartSearch && e.key === 'Enter') {
      e.preventDefault();
      handleAiSearch();
    }
  };

  const handleCheckout = async (bookId) => {
    try {
      await checkoutBook(bookId);
      setMessage('Book checked out successfully!');
      if (smartSearch && aiResults) {
        handleAiSearch(); // refresh AI results
      } else {
        fetchBooks();
      }
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
      if (smartSearch && aiResults) {
        handleAiSearch();
      } else {
        fetchBooks();
      }
    } catch (err) {
      setMessage('Failed to delete book');
      setTimeout(() => setMessage(''), 3000);
    }
  };

  const handleSave = () => {
    setShowModal(false);
    setEditBook(null);
    if (smartSearch && aiResults) {
      handleAiSearch();
    } else {
      fetchBooks();
    }
  };

  // Determine which books to display
  const displayBooks = smartSearch && aiResults
    ? aiResults.items.map(item => ({ ...item.book, _aiScore: item.score, _aiReason: item.reason }))
    : books;

  const showPagination = !smartSearch && totalPages > 1;

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
          placeholder={smartSearch
            ? 'Describe what you\'re looking for, e.g. "a book about dystopian society with surveillance"...'
            : 'Search books by title, author, ISBN, genre...'}
          value={query}
          onChange={(e) => {
            setQuery(e.target.value);
            if (!smartSearch) setPage(1);
          }}
          onKeyDown={handleKeyDown}
        />
        {smartSearch && (
          <button
            className="btn btn-primary"
            onClick={handleAiSearch}
            disabled={aiLoading || !query.trim()}
          >
            {aiLoading ? '🔍 Searching...' : '🔍 AI Search'}
          </button>
        )}
        {aiAvailable && (
          <div className="smart-search-toggle">
            <label className="toggle-label">
              <input
                type="checkbox"
                checked={smartSearch}
                onChange={(e) => setSmartSearch(e.target.checked)}
              />
              <span className="toggle-slider"></span>
              <span className="toggle-text">✨ Smart Search (AI)</span>
            </label>
          </div>
        )}
      </div>

      {smartSearch && (
        <div className="ai-search-hint">
          💡 Smart Search uses AI to understand your query in natural language. Press <strong>Enter</strong> or click <strong>AI Search</strong> to find matching books.
        </div>
      )}

      {aiError && <div className="alert alert-error">{aiError}</div>}

      {aiLoading && (
        <div className="ai-loading">
          <div className="ai-loading-spinner"></div>
          <p>AI is analyzing your query against the library catalog...</p>
        </div>
      )}

      <div className="book-grid">
        {displayBooks.map(book => (
          <div key={book.id} className="book-card">
            {book._aiScore !== undefined && (
              <div className="ai-match-badge">
                <span className="ai-score">✨ {Math.round(book._aiScore * 100)}% match</span>
              </div>
            )}
            <h3>{book.title}</h3>
            <div className="author">by {book.author}</div>
            <div className="meta">ISBN: {book.isbn} · {book.publicationYear} · {book.publisher}</div>
            <div className="meta">Genre: {book.genre}</div>
            <div className="description">{book.description}</div>
            {book._aiReason && (
              <div className="ai-reason">
                <strong>Why this matches:</strong> {book._aiReason}
              </div>
            )}
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

      {displayBooks.length === 0 && !aiLoading && (
        <p style={{ textAlign: 'center', color: '#718096', marginTop: 40 }}>
          {smartSearch ? 'No AI results yet. Enter a query and press Enter to search.' : 'No books found.'}
        </p>
      )}

      {showPagination && (
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
