import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7074/api', // Use HTTPS directly to avoid 307 redirect dropping the Authorization header
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to attach the JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token && token !== 'undefined' && token !== 'null') {
      config.headers.set('Authorization', `Bearer ${token}`);
    } else {
      localStorage.removeItem('token');
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default api;
