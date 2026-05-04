import axios, { AxiosError, type AxiosResponse } from 'axios';

// API Response types
export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
    errors?: string[];
    timestamp?: string;
}

// Error types for better categorization
export type ApiErrorType =
    | 'NETWORK_ERROR'
    | 'TIMEOUT_ERROR'
    | 'UNAUTHORIZED'
    | 'FORBIDDEN'
    | 'NOT_FOUND'
    | 'VALIDATION_ERROR'
    | 'CONFLICT'
    | 'SERVER_ERROR'
    | 'UNKNOWN_ERROR';

export interface ApiError {
    type: ApiErrorType;
    status?: number;
    title: string;
    message: string;
    errors?: string[];
}

// Helper function to check if error is network/connectivity related
const isNetworkError = (error: AxiosError): boolean => {
    const message = error.message?.toLowerCase() || '';
    const code = error.code || '';

    // Check for common network error patterns
    return (
        code === 'ERR_NETWORK' ||
        code === 'ENOTFOUND' ||
        code === 'ECONNREFUSED' ||
        code === 'ECONNRESET' ||
        message === 'network error' ||
        message.includes('no such host') ||
        message.includes('network') ||
        message.includes('internet') ||
        message.includes('offline') ||
        message.includes('dns') ||
        message.includes('getaddrinfo') ||
        message.includes('enotfound') ||
        message.includes('econnrefused')
    );
};

// Error message mapping for user-friendly messages
const getErrorDetails = (error: AxiosError<ApiResponse<unknown>>): ApiError => {
    // Network error (no response received)
    if (!error.response) {
        // Check for network/connectivity errors
        if (isNetworkError(error)) {
            return {
                type: 'NETWORK_ERROR',
                title: 'No Internet Connection',
                message: 'Please check your internet connection and try again.',
            };
        }
        if (error.code === 'ECONNABORTED' || error.message.includes('timeout')) {
            return {
                type: 'TIMEOUT_ERROR',
                title: 'Request Timeout',
                message: 'The server took too long to respond. Please try again later.',
            };
        }
        // Default fallback for unknown network issues
        return {
            type: 'NETWORK_ERROR',
            title: 'Connection Error',
            message: 'Unable to connect to the server. Please check your internet connection.',
        };
    }

    const status = error.response.status;
    const apiResponse = error.response.data;
    const serverMessage = apiResponse?.message || '';
    const serverErrors = apiResponse?.errors || [];

    switch (status) {
        case 400:
            return {
                type: 'VALIDATION_ERROR',
                status,
                title: 'Invalid Request',
                message: serverMessage || 'The request contains invalid data. Please check your input and try again.',
                errors: serverErrors,
            };
        case 401:
            return {
                type: 'UNAUTHORIZED',
                status,
                title: 'Session Expired',
                message: serverMessage || 'Your session has expired. Please log in again.',
                errors: serverErrors,
            };
        case 403:
            return {
                type: 'FORBIDDEN',
                status,
                title: 'Access Denied',
                message: serverMessage || 'You do not have permission to perform this action.',
                errors: serverErrors,
            };
        case 404:
            return {
                type: 'NOT_FOUND',
                status,
                title: 'Not Found',
                message: serverMessage || 'The requested resource was not found.',
                errors: serverErrors,
            };
        case 409:
            return {
                type: 'CONFLICT',
                status,
                title: 'Conflict',
                message: serverMessage || 'This action conflicts with existing data. Please try a different value.',
                errors: serverErrors,
            };
        case 422:
            return {
                type: 'VALIDATION_ERROR',
                status,
                title: 'Validation Failed',
                message: serverMessage || 'The provided data failed validation. Please check your input.',
                errors: serverErrors,
            };
        case 429:
            return {
                type: 'SERVER_ERROR',
                status,
                title: 'Too Many Requests',
                message: 'You have made too many requests. Please wait a moment and try again.',
                errors: serverErrors,
            };
        case 500:
            return {
                type: 'SERVER_ERROR',
                status,
                title: 'Server Error',
                message: serverMessage || 'An unexpected error occurred on the server. Please try again later.',
                errors: serverErrors,
            };
        case 502:
        case 503:
        case 504:
            return {
                type: 'SERVER_ERROR',
                status,
                title: 'Service Unavailable',
                message: 'The server is temporarily unavailable. Please try again in a few minutes.',
                errors: serverErrors,
            };
        default:
            return {
                type: 'UNKNOWN_ERROR',
                status,
                title: 'Error',
                message: serverMessage || 'An unexpected error occurred. Please try again.',
                errors: serverErrors,
            };
    }
};

// Event emitter for toast notifications
type ToastListener = (error: ApiError) => void;
let toastListener: ToastListener | null = null;

export const setApiErrorListener = (listener: ToastListener | null) => {
    toastListener = listener;
};

export const notifyError = (error: ApiError) => {
    if (toastListener) {
        toastListener(error);
    }
};

// Create axios instance
const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5068/api/v1',
    timeout: 30000, // 30 second timeout
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor - add auth token
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Response interceptor - handle errors globally
api.interceptors.response.use(
    (response: AxiosResponse) => response,
    (error: AxiosError<ApiResponse<unknown>>) => {
        const apiError = getErrorDetails(error);

        // Handle 401 - redirect to login
        if (apiError.type === 'UNAUTHORIZED') {
            localStorage.removeItem('token');
            localStorage.removeItem('user');

            // Only redirect if not already on login page
            if (!window.location.pathname.includes('/login')) {
                window.location.href = '/login';
            }
        }

        // Notify toast listener (if set)
        notifyError(apiError);

        // Attach our parsed error to the axios error for consumers
        (error as any).apiError = apiError;

        return Promise.reject(error);
    }
);

export default api;
