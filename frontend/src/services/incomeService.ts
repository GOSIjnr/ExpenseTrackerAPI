import type { ApiResponse } from './api';
import api from './api';

export const IncomeSource = {
    Salary: 'Salary',
    Freelance: 'Freelance',
    Business: 'Business',
    Investments: 'Investments',
    Rental: 'Rental',
    Gift: 'Gift',
    Refund: 'Refund',
    Other: 'Other'
} as const;

export type IncomeSource = typeof IncomeSource[keyof typeof IncomeSource];

export const RecurrenceFrequency = {
    Weekly: 'Weekly',
    BiWeekly: 'BiWeekly',
    Monthly: 'Monthly',
    Quarterly: 'Quarterly',
    Annually: 'Annually'
} as const;

export type RecurrenceFrequency = typeof RecurrenceFrequency[keyof typeof RecurrenceFrequency];

export interface Income {
    id: string;
    source: IncomeSource;
    sourceName: string;
    amount: number;
    dateReceived: string;
    description?: string;
    isRecurring: boolean;
    frequency?: RecurrenceFrequency;
    frequencyName?: string;
    createdAt: string;
}

export interface CreateIncomeRequest {
    source: IncomeSource;
    amount: number;
    dateReceived: string;
    description?: string;
    isRecurring?: boolean;
    frequency?: RecurrenceFrequency;
}

export interface UpdateIncomeRequest {
    id: string;
    source?: IncomeSource;
    amount?: number;
    dateReceived?: string;
    description?: string;
    isRecurring?: boolean;
    frequency?: RecurrenceFrequency;
}

export interface IncomeBySource {
    source: IncomeSource;
    sourceName: string;
    totalAmount: number;
    percentage: number;
}

export interface IncomeSummary {
    totalMonthlyIncome: number;
    totalAllTimeIncome: number;
    savingsRate: number;
    netCashFlow: number;
    incomeBySource: IncomeBySource[];
}

// API functions
export const incomeService = {
    getAll: async (): Promise<Income[]> => {
        const response = await api.get<ApiResponse<Income[]>>('/income');
        return response.data.data || [];
    },

    getById: async (id: string): Promise<Income | null> => {
        const response = await api.get<ApiResponse<Income>>(`/income/${id}`);
        return response.data.data || null;
    },

    create: async (request: CreateIncomeRequest): Promise<Income> => {
        const response = await api.post<ApiResponse<Income>>('/income', request);
        if (!response.data.success || !response.data.data) {
            throw new Error(response.data.message || 'Failed to create income');
        }
        return response.data.data;
    },

    update: async (id: string, request: UpdateIncomeRequest): Promise<void> => {
        const response = await api.put<ApiResponse<null>>(`/income/${id}`, request);
        if (!response.data.success) {
            throw new Error(response.data.message || 'Failed to update income');
        }
    },

    delete: async (id: string): Promise<void> => {
        const response = await api.delete<ApiResponse<null>>(`/income/${id}`);
        if (!response.data.success) {
            throw new Error(response.data.message || 'Failed to delete income');
        }
    },

    getSummary: async (month?: number, year?: number): Promise<IncomeSummary> => {
        const params = new URLSearchParams();
        if (month) params.append('month', month.toString());
        if (year) params.append('year', year.toString());

        const url = params.toString() ? `/income/summary?${params}` : '/income/summary';
        const response = await api.get<ApiResponse<IncomeSummary>>(url);
        return response.data.data || {
            totalMonthlyIncome: 0,
            totalAllTimeIncome: 0,
            savingsRate: 0,
            netCashFlow: 0,
            incomeBySource: []
        };
    }
};

// Helper to get icon for income source (no emojis)
export const getIncomeSourceIcon = (_source: IncomeSource): string => {
    return '';
};

// Helper to get color for income source
export const getIncomeSourceColor = (source: IncomeSource): string => {
    const colors: Record<IncomeSource, string> = {
        [IncomeSource.Salary]: '#3b82f6',
        [IncomeSource.Freelance]: '#8b5cf6',
        [IncomeSource.Business]: '#10b981',
        [IncomeSource.Investments]: '#f59e0b',
        [IncomeSource.Rental]: '#06b6d4',
        [IncomeSource.Gift]: '#ec4899',
        [IncomeSource.Refund]: '#14b8a6',
        [IncomeSource.Other]: '#64748b'
    };
    return colors[source] || '#64748b';
};
