import api from './api';

// Types
export interface FinancialHealthScore {
    totalScore: number;
    savingsScore: number;
    budgetScore: number;
    goalScore: number;
    trendScore: number;
    emergencyScore: number;
    rating: string;
    trend: string;
    recommendations: string[];
}

export interface SpendingPatterns {
    spendingByDayOfWeek: Record<string, number>;
    categoryTrends: CategoryTrend[];
    recurringExpenses: RecurringExpense[];
    anomalies: Anomaly[];
}

export interface CategoryTrend {
    category: string;
    currentMonthTotal: number;
    lastMonthTotal: number;
    changePercentage: number;
    trend: string;
    averageTransactionSize: number;
    transactionCount: number;
}

export interface RecurringExpense {
    description: string;
    amount: number;
    frequency: string;
    lastOccurrence: string;
    nextExpectedDate?: string;
}

export interface Anomaly {
    date: string;
    category: string;
    amount: number;
    averageAmount: number;
    deviationPercentage: number;
    reason: string;
}

export interface SpendingForecast {
    projectedMonthEnd: number;
    currentSpending: number;
    daysElapsed: number;
    daysRemaining: number;
    dailyAverage: number;
    projectedAdditionalSpending: number;
    categoryForecasts: CategoryForecast[];
}

export interface CategoryForecast {
    category: string;
    current: number;
    projected: number;
    budgetLimit: number;
    willExceedBudget: boolean;
    excessAmount: number;
}

export interface PredictiveInsights {
    budgetWarnings: BudgetWarning[];
    goalPredictions: GoalPrediction[];
    recommendations: Recommendation[];
    savingsOpportunities: SavingsOpportunity[];
}

export interface BudgetWarning {
    category: string;
    budgetLimit: number;
    currentSpending: number;
    projectedTotal: number;
    excessAmount: number;
    severity: string;
    message: string;
    suggestedAction?: string;
}

export interface GoalPrediction {
    goalTitle: string;
    targetAmount: number;
    currentAmount: number;
    targetDate?: string;
    projectedCompletionDate?: string;
    status: string;
    monthlyContributionNeeded: number;
    message: string;
    suggestedAction?: string;
}

export interface Recommendation {
    type: string;
    category: string;
    message: string;
    suggestedAction?: string;
    suggestedAmount?: number;
    priority: string;
}

export interface SavingsOpportunity {
    category: string;
    currentSpending: number;
    recommendedReduction: number;
    potentialMonthlySavings: number;
    message: string;
    suggestedAction?: string;
}

// Service
// Note: Analytics controller uses /api/Analytics (not versioned), so we use full URL
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5068/api/v1';
// Remove /v1 from the end if it exists, as Analytics controller is mapped to /api/Analytics
const API_ROOT = BASE_URL.replace(/\/v1$/, '');
const ANALYTICS_BASE = `${API_ROOT}/Analytics`;

export const analyticsService = {
    getHealthScore: async (): Promise<FinancialHealthScore> => {
        const response = await api.get<FinancialHealthScore>(`${ANALYTICS_BASE}/health-score`, {
            baseURL: '' // Override the default baseURL
        });
        return response.data;
    },

    getSpendingPatterns: async (): Promise<SpendingPatterns> => {
        const response = await api.get<SpendingPatterns>(`${ANALYTICS_BASE}/spending-patterns`, {
            baseURL: ''
        });
        return response.data;
    },

    getCategoryTrends: async (): Promise<CategoryTrend[]> => {
        const response = await api.get<CategoryTrend[]>(`${ANALYTICS_BASE}/category-trends`, {
            baseURL: ''
        });
        return response.data;
    },

    getForecast: async (): Promise<SpendingForecast> => {
        const response = await api.get<SpendingForecast>(`${ANALYTICS_BASE}/forecast`, {
            baseURL: ''
        });
        return response.data;
    },

    getPredictions: async (): Promise<PredictiveInsights> => {
        const response = await api.get<PredictiveInsights>(`${ANALYTICS_BASE}/predictions`, {
            baseURL: ''
        });
        return response.data;
    },

    getAchievements: async (): Promise<Achievement[]> => {
        const response = await api.get<Achievement[]>(`${ANALYTICS_BASE}/achievements`, {
            baseURL: ''
        });
        return response.data;
    },
};

export interface Achievement {
    id: string;
    title: string;
    description: string;
    icon: string;
    earned: boolean;
    progress: number;
    total: number;
}
