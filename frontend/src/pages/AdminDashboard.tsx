import React, { useState, useEffect } from 'react';
import { classApi } from '../api/services';
import { ClassOffering } from '../types';
import { Loading, ErrorMessage } from '../components/Common';

const AdminDashboard = () => {
    const [classes, setClasses] = useState<ClassOffering[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchClasses = async () => {
            try {
                const data = await classApi.getAll();
                setClasses(data);
            } catch (err: any) {
                setError(err.message || 'Failed to load classes');
            } finally {
                setLoading(false);
            }
        };
        fetchClasses();
    }, []);

    const totals = classes.reduce((acc, cls) => ({
        total: acc.total + cls.totalSeats,
        available: acc.available + cls.availableSeats,
    }), { total: 0, available: 0 });

    if (loading) return <Loading />;
    if (error) return <ErrorMessage message={error} />;

    return (
        <div className="space-y-8">
            <h2 className="text-2xl font-bold">Admin Dashboard</h2>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div className="bg-white p-6 rounded-lg shadow border-l-4 border-blue-600">
                    <p className="text-sm text-gray-500 uppercase font-semibold">Total Classes</p>
                    <p className="text-3xl font-bold">{classes.length}</p>
                </div>
                <div className="bg-white p-6 rounded-lg shadow border-l-4 border-green-600">
                    <p className="text-sm text-gray-500 uppercase font-semibold">Total Seats</p>
                    <p className="text-3xl font-bold">{totals.total}</p>
                </div>
                <div className="bg-white p-6 rounded-lg shadow border-l-4 border-yellow-600">
                    <p className="text-sm text-gray-500 uppercase font-semibold">Available Seats</p>
                    <p className="text-3xl font-bold">{totals.available}</p>
                </div>
                <div className="bg-white p-6 rounded-lg shadow border-l-4 border-red-600">
                    <p className="text-sm text-gray-500 uppercase font-semibold">Occupied Seats</p>
                    <p className="text-3xl font-bold">{totals.total - totals.available}</p>
                </div>
            </div>

            <div className="flex justify-end">
                <a href="/admin/create-class" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 font-medium transition-colors">
                    + Create New Class
                </a>
            </div>
        </div>
    );
};

export default AdminDashboard;
