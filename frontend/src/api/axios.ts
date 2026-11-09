import axios from 'axios';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || 'http://localhost:8080',
    headers: {
        'Content-Type': 'application/json',
    },
});

// Interceptor to add X-Student-Id header from localStorage
api.interceptors.request.use((config) => {
    const studentId = localStorage.getItem('student_id');
    if (studentId) {
        config.headers['X-Student-Id'] = studentId;
    }
    return config;
});

// Global error handling
api.interceptors.response.use(
    (response) => response,
    (error) => {
        const message = error.response?.data?.message || 'An unexpected error occurred';
        console.error('API Error:', message);
        return Promise.reject(error);
    }
);

export default api;
