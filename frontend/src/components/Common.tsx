import React from 'react';
import { Loader2, AlertCircle, CheckCircle2 } from 'lucide-react';

export const Loading = () => (
    <div className="flex flex-col items-center justify-center p-12 space-y-4">
        <Loader2 className="h-10 w-10 text-academic-600 animate-spin" />
        <p className="text-academic-500 font-medium animate-pulse">Loading your data...</p>
    </div>
);

export const ErrorMessage = ({ message }: { message: string }) => (
    <div className="flex items-start p-4 mb-6 bg-danger-50 border-l-4 border-danger-500 text-danger-700 rounded-r-lg shadow-sm">
        <AlertCircle className="h-5 w-5 mr-3 mt-0.5 flex-shrink-0" />
        <div>
            <p className="font-bold">Something went wrong</p>
            <p className="text-sm">{message}</p>
        </div>
    </div>
);

export const SuccessMessage = ({ message }: { message: string }) => (
    <div className="flex items-start p-4 mb-6 bg-success-50 border-l-4 border-success-500 text-success-700 rounded-r-lg shadow-sm">
        <CheckCircle2 className="h-5 w-5 mr-3 mt-0.5 flex-shrink-0" />
        <div>
            <p className="font-bold">Success!</p>
            <p className="text-sm">{message}</p>
        </div>
    </div>
);
