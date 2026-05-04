import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import { CheckCircle, XCircle, AlertTriangle, Info, X, WifiOff } from 'lucide-react';

// Toast types
export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
    id: string;
    type: ToastType;
    title: string;
    message: string;
    duration?: number;
}

interface ToastContextType {
    toasts: Toast[];
    addToast: (type: ToastType, title: string, message: string, duration?: number) => void;
    removeToast: (id: string) => void;
    success: (title: string, message?: string) => void;
    error: (title: string, message?: string) => void;
    warning: (title: string, message?: string) => void;
    info: (title: string, message?: string) => void;
    networkError: () => void;
}

const ToastContext = createContext<ToastContextType | null>(null);

export const useToast = () => {
    const context = useContext(ToastContext);
    if (!context) {
        throw new Error('useToast must be used within a ToastProvider');
    }
    return context;
};

// Toast Icons
const toastIcons = {
    success: CheckCircle,
    error: XCircle,
    warning: AlertTriangle,
    info: Info,
};

// Toast Styles
const toastStyles = {
    success: {
        bg: 'bg-emerald-500/10',
        border: 'border-emerald-500/30',
        icon: 'text-emerald-500',
        title: 'text-emerald-400',
    },
    error: {
        bg: 'bg-rose-500/10',
        border: 'border-rose-500/30',
        icon: 'text-rose-500',
        title: 'text-rose-400',
    },
    warning: {
        bg: 'bg-amber-500/10',
        border: 'border-amber-500/30',
        icon: 'text-amber-500',
        title: 'text-amber-400',
    },
    info: {
        bg: 'bg-blue-500/10',
        border: 'border-blue-500/30',
        icon: 'text-blue-500',
        title: 'text-blue-400',
    },
};

// Individual Toast Component
const ToastItem = ({ toast, onRemove }: { toast: Toast; onRemove: () => void }) => {
    const Icon = toastIcons[toast.type];
    const styles = toastStyles[toast.type];

    return (
        <div
            className={`
                flex items-start gap-3 p-4 rounded-xl border backdrop-blur-sm
                ${styles.bg} ${styles.border}
                animate-in slide-in-from-right fade-in duration-300
                shadow-lg shadow-black/20
            `}
        >
            <div className={`flex-shrink-0 ${styles.icon}`}>
                <Icon size={20} />
            </div>
            <div className="flex-1 min-w-0">
                <h4 className={`font-semibold text-sm ${styles.title}`}>
                    {toast.title}
                </h4>
                {toast.message && (
                    <p className="text-gray-400 text-sm mt-0.5 break-words">
                        {toast.message}
                    </p>
                )}
            </div>
            <button
                onClick={onRemove}
                className="flex-shrink-0 text-gray-500 hover:text-white transition-colors"
            >
                <X size={16} />
            </button>
        </div>
    );
};

// Network Status Indicator
const NetworkStatusIndicator = ({ isOnline }: { isOnline: boolean }) => {
    if (isOnline) return null;

    return (
        <div className="fixed top-0 left-0 right-0 z-[60] bg-rose-600 text-white py-2 px-4 flex items-center justify-center gap-2 text-sm font-medium">
            <WifiOff size={16} />
            <span>You are offline. Some features may not work properly.</span>
        </div>
    );
};

// Toast Provider Component
export const ToastProvider = ({ children }: { children: ReactNode }) => {
    const [toasts, setToasts] = useState<Toast[]>([]);
    const [isOnline, setIsOnline] = useState(navigator.onLine);

    // Monitor network status
    useState(() => {
        const handleOnline = () => setIsOnline(true);
        const handleOffline = () => setIsOnline(false);

        window.addEventListener('online', handleOnline);
        window.addEventListener('offline', handleOffline);

        return () => {
            window.removeEventListener('online', handleOnline);
            window.removeEventListener('offline', handleOffline);
        };
    });

    const addToast = useCallback((type: ToastType, title: string, message: string, duration = 5000) => {
        const id = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

        setToasts(prev => [...prev, { id, type, title, message, duration }]);

        // Auto remove after duration
        if (duration > 0) {
            setTimeout(() => {
                setToasts(prev => prev.filter(t => t.id !== id));
            }, duration);
        }
    }, []);

    const removeToast = useCallback((id: string) => {
        setToasts(prev => prev.filter(t => t.id !== id));
    }, []);

    // Convenience methods
    const success = useCallback((title: string, message = '') => {
        addToast('success', title, message);
    }, [addToast]);

    const error = useCallback((title: string, message = '') => {
        addToast('error', title, message, 7000); // Errors stay longer
    }, [addToast]);

    const warning = useCallback((title: string, message = '') => {
        addToast('warning', title, message);
    }, [addToast]);

    const info = useCallback((title: string, message = '') => {
        addToast('info', title, message);
    }, [addToast]);

    const networkError = useCallback(() => {
        addToast(
            'error',
            'Connection Error',
            'Unable to connect to the server. Please check your internet connection and try again.',
            10000
        );
    }, [addToast]);

    return (
        <ToastContext.Provider value={{ toasts, addToast, removeToast, success, error, warning, info, networkError }}>
            <NetworkStatusIndicator isOnline={isOnline} />
            {children}

            {/* Toast Container */}
            <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-3 max-w-sm w-full pointer-events-none">
                {toasts.map(toast => (
                    <div key={toast.id} className="pointer-events-auto">
                        <ToastItem
                            toast={toast}
                            onRemove={() => removeToast(toast.id)}
                        />
                    </div>
                ))}
            </div>
        </ToastContext.Provider>
    );
};
