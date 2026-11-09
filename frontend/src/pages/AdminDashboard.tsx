import { useState, useEffect } from 'react';
import { classApi } from '../api/services';
import { type ClassOffering } from '../types';
import { Loading, ErrorMessage } from '../components/Common';

const AdminDashboard = () => {
    const [classes, setClasses] = useState<ClassOffering[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let active = true;
        const fetchClasses = async () => {
            try {
                const data = await classApi.getAll();
                if (active) { setClasses(data); setError(null); }
            } catch (err: any) {
                if (active) setError(err.response?.data?.error || err.message || 'Failed to load classes');
            } finally {
                if (active) setLoading(false);
            }
        };
        fetchClasses();
        const refresh = setInterval(fetchClasses, 5000);
        return () => { active = false; clearInterval(refresh); };
    }, []);

    const totals = classes.reduce((acc, cls) => ({
        total: acc.total + cls.totalSeats,
        available: acc.available + cls.availableSeats,
        held: acc.held + cls.heldSeats,
        confirmed: acc.confirmed + cls.confirmedSeats,
    }), { total: 0, available: 0, held: 0, confirmed: 0 });

    if (loading) return <Loading />;
    if (error) return <ErrorMessage message={error} />;

    return (
        <div className="space-y-8">
            <h2 className="text-2xl font-bold">Admin Dashboard</h2>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
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
                <div className="bg-white p-6 rounded-lg shadow border-l-4 border-yellow-600">
                    <p className="text-sm text-gray-500 uppercase font-semibold">Held Seats</p>
                    <p className="text-3xl font-bold">{totals.held}</p>
                </div>
                <div className="bg-white p-6 rounded-lg shadow border-l-4 border-red-600">
                    <p className="text-sm text-gray-500 uppercase font-semibold">Confirmed Seats</p>
                    <p className="text-3xl font-bold">{totals.confirmed}</p>
                </div>
            </div>

            <div className="flex justify-end">
                <a href="/admin/create-class" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 font-medium transition-colors">
                    + Create New Class
                </a>
            </div>
            <div className="overflow-x-auto bg-white rounded-lg shadow">
                <table className="w-full text-left">
                    <thead className="bg-gray-100"><tr><th className="p-3">Class / Shift</th><th className="p-3">Capacity</th><th className="p-3">Held</th><th className="p-3">Confirmed</th><th className="p-3">Available</th></tr></thead>
                    <tbody>{classes.map(cls => <tr key={cls.id} className="border-t"><td className="p-3">{cls.name}</td><td className="p-3">{cls.totalSeats}</td><td className="p-3">{cls.heldSeats}</td><td className="p-3">{cls.confirmedSeats}</td><td className="p-3">{cls.availableSeats}</td></tr>)}</tbody>
                </table>
                {classes.length === 0 && <p className="p-6 text-center text-gray-500">No classes have been created yet.</p>}
            </div>
        </div>
    );
};

export default AdminDashboard;
