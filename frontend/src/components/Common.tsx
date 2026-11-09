import React from 'react';

export const Loading = () => (
    <div className="flex items-center justify-center p-4">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
);

export const ErrorMessage = ({ message }: { message: string }) => (
    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative mb-4">
        <strong className="font-bold">Error: </strong>
        <span>{message}</span>
    </div>
);

export const Notification = ({ message, type = 'success' }: { message: string, type?: 'success' | 'error' }) => (
    <div className={`fixed bottom-4 right-4 px-6 py-3 rounded-lg shadow-lg text-white transition-all ${type === 'success' ? 'bg-green-600' : 'bg-red-600'}`}>
        {message}
    </div>
);
