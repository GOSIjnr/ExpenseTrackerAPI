import { useEffect } from 'react';
import { useToast } from '../context/ToastContext';
import { setApiErrorListener, type ApiError } from '../services/api';

/**
 * Hook to connect API errors to toast notifications.
 * Should be used once in the app, preferably in the main Layout component.
 */
export const useApiErrorHandler = () => {
    const toast = useToast();

    useEffect(() => {
        const handleApiError = (error: ApiError) => {
            // Build the full error message
            let message = error.message;

            // Append additional errors if present
            if (error.errors && error.errors.length > 0) {
                message = error.errors.join('. ');
            }

            // Use appropriate toast type based on error type
            switch (error.type) {
                case 'NETWORK_ERROR':
                case 'TIMEOUT_ERROR':
                    toast.error(error.title, message);
                    break;
                case 'VALIDATION_ERROR':
                    toast.warning(error.title, message);
                    break;
                case 'UNAUTHORIZED':
                    // Only show toast if not auto-redirecting
                    toast.info(error.title, message);
                    break;
                case 'NOT_FOUND':
                    toast.warning(error.title, message);
                    break;
                case 'CONFLICT':
                    toast.warning(error.title, message);
                    break;
                case 'SERVER_ERROR':
                    toast.error(error.title, message);
                    break;
                default:
                    toast.error(error.title, message);
            }
        };

        // Register the error handler
        setApiErrorListener(handleApiError);

        // Cleanup on unmount
        return () => {
            setApiErrorListener(null);
        };
    }, [toast]);
};
