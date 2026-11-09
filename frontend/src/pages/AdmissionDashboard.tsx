import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { classApi } from '../api/services';
import { ClassOffering } from '../types';
import { Loading, ErrorMessage } from '../components/Common';

const AdmissionDashboard = () => {
    const [classes, setClasses] = useState<ClassOffering[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

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

    const handleReserve = async (classId: string) => {
        const studentId = localStorage.getItem('student_id');
        if (!studentId) {
            navigate('/student-id');
            return;
        }

        try {
            const hold = await admissionApi.createHold(classId);
            navigate(`/reservation/${hold.id}`);
        } catch (err: any) {
            alert(err.message || 'Reservation failed');
        }
    };

    if (loading) return <Loading />;
    if (error) return <ErrorMessage message={error} />;

    return (
        <div>
            <h2 className="text-2xl font-bold mb-6">Available Classes</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {classes.map(cls => (
                    <div key={cls.id} className="bg-white p-6 rounded-lg shadow-md border border-gray-200">
                        <h3 className="text-xl font-semibold mb-2">{cls.className}</h3>
                        <div className="space-y-1 text-sm text-gray-600 mb-4">
                            <p>Total Seats: {cls.totalSeats}</p>
                            <p>Available Seats: <span className={cls.availableSeats === 0 ? 'text-red-500 font-bold' : ''}>{cls.availableSeats}</span></p>
                        </div>
                        <button
                            onClick={() => handleReserve(cls.id)}
                            disabled={cls.availableSeats === 0}
                            className={`w-full py-2 px-4 rounded font-medium transition-colors ${
                                cls.availableSeats === 0
                                ? 'bg-gray-300 cursor-not-allowed'
                                : 'bg-blue-600 text-white hover:bg-blue-700'
                            }`}
                        >
                            {cls.availableSeats === 0 ? 'No Seats Available' : 'Reserve Seat'}
                        </button>
                    </div>
                ))}
            </div>
        </div>
    );
};

// Mocking admissionApi for the dashboard since it's used but not yet imported
const admissionApi = {
    createHold: async (classId: string) => {
        const api = (await import('../api/axios')).default;
        const { data } = await api.post(`/classes/${classId}/holds`);
        return data;
    }
};

export default AdmissionDashboard;
