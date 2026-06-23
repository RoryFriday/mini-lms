import axios from 'axios';

const API_BASE = process.env.REACT_APP_API_URL || '';

const api = axios.create({
  baseURL: `${API_BASE}/api`,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth
export const login = (data) => api.post('/auth/login', data);
export const register = (data) => api.post('/auth/register', data);
export const getMe = () => api.get('/auth/me');
export const getUsers = () => api.get('/auth/users');
export const updateUserRole = (id, role) => api.put(`/auth/users/${id}/role`, JSON.stringify(role), {
  headers: { 'Content-Type': 'application/json' }
});

// Books
export const searchBooks = (params) => api.get('/books', { params });
export const getBook = (id) => api.get(`/books/${id}`);
export const createBook = (data) => api.post('/books', data);
export const updateBook = (id, data) => api.put(`/books/${id}`, data);
export const deleteBook = (id) => api.delete(`/books/${id}`);

// AI Search
export const aiSearchStatus = () => api.get('/books/ai-search/status');
export const aiSearchBooks = (query) => api.post('/books/ai-search', { query });

// Checkouts
export const checkoutBook = (bookId) => api.post('/checkouts', { bookId });
export const returnBook = (recordId) => api.post(`/checkouts/${recordId}/return`);
export const returnBookOnBehalf = (recordId) => api.post(`/checkouts/${recordId}/return-on-behalf`);
export const getMyCheckouts = (activeOnly = false) => api.get('/checkouts/my', { params: { activeOnly } });
export const getAllCheckouts = (activeOnly = false) => api.get('/checkouts', { params: { activeOnly } });

export default api;
