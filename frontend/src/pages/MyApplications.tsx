import { useState, useEffect } from 'react';
import { admissionApi } from '../api/services';
import { type AdmissionHold } from '../types';
import { Loading, ErrorMessage } from '../components/Common';
import { Link } from 'react-router-dom';

const MyApplications = () => {
    const [holds, setHolds] = useState<AdmissionHold[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let active = true;
        const fetchHolds = async () => {
            try {
                const data = await admissionApi.getMyHolds();
                if (active) { setHolds(data); setError(null); }
            } catch (err: any) {
                if (active) setError(err.response?.data?.error || err.message || 'Failed to load applications');
            } finally {
                if (active) setLoading(false);
            }
        };
        fetchHolds();
        const refresh = setInterval(fetchHolds, 10000);
        return () => { active = false; clearInterval(refresh); };
    }, []);

    if (loading) return <Loading />;
    if (error) return <ErrorMessage message={error} />;

    return (
        <div>
            <h2 className="text-2xl font-bold mb-6">My Applications</h2>
            {holds.length === 0 ? (
                <div className="text-center py-10 text-gray-500">No applications found.</div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse bg-white rounded-lg shadow-sm">
                        <thead className="bg-gray-100 border-b">
                            <tr>
                                <th className="p-4 font-semibold">Reservation ID</th>
                                <th className="p-4 font-semibold">Class</th>
                                <th className="p-4 font-semibold">Status</th>
                                <th className="p-4 font-semibold">Expires At</th>
                                <th className="p-4 font-semibold">Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            {holds.map(hold => (
                                <tr key={hold.holdId} className="border-b hover:bg-gray-50 transition-colors">
                                    <td className="p-4 font-mono text-xs">{hold.holdId}</td>
                                    <td className="p-4">{hold.className}</td>
                                    <td className="p-4">
                                        <span className={`px-2 py-1 rounded text-xs font-bold ${
                                            hold.status === 'CONFIRMED' ? 'bg-green-100 text-green-700' : hold.status === 'HELD' ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-700'
                                        }`}>
                                            {hold.status}
                                        </span>
                                    </td>
                                    <td className="p-4 text-sm text-gray-600">{new Date(hold.expiresAt).toLocaleString()}</td>
                                    <td className="p-4">
                                        <Link to={`/reservation/${hold.holdId}`} className="text-blue-600 hover:underline text-sm font-medium">
                                            View Details
                                        </Link>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
};

export default MyApplications;
