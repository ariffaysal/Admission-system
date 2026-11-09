import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { classApi } from '../api/services';
import { ClassOffering } from '../types';
import { Loading, ErrorMessage } from '../components/Common';
import { BookOpen, Users, ArrowRight, Info } from 'lucide-react';
import { motion } from 'framer-motion';
import toast from 'react-hot-toast';

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
            toast.error('Please identify your student first');
            navigate('/student-id');
            return;
        }

        try {
            const api = (await import('../api/axios')).default;
            const { data: hold } = await api.post(`/classes/${classId}/holds`);
            toast.success('Seat reserved successfully!');
            navigate(`/reservation/${hold.id}`);
        } catch (err: any) {
            toast.error(err.response?.data?.message || 'Reservation failed');
        }
    };

    if (loading) return <Loading />;
    if (error) return <ErrorMessage message={error} />;

    return (
        <div className="max-w-6xl mx-auto space-y la">
            <header className="mb-10 text-center">
                <h2 className="text-4xl font-extrabold text-academic-900 mb-3">School Admission 2026</h2>
                <p className="text-academic-500 text-lg max-w-2xl mx-auto">
                    Welcome, Parent. Please select a class to reserve a seat for your child.
                    Seats are allocated on a <span className="font-bold text-academic-700">first-come, first-served</span> basis.
                </p>
            </header>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap la">
                {classes.map((cls, index) => (
                    <motion.div
                        key={cls.id}
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: index * 0.1 }}
                        className="bg-white rounded-2xl shadow-sm border border-academic-200 hover:shadow-xl transition-all duration-300 group overflow-hidden"
                    >
                        <div className="p-6">
                            <div className="flex justify-between items-start mb-4">
                                <div className="p-3 bg-academic-100 rounded-xl text-academic-600 group-hover:bg-academic-600 group-hover:text-white transition-colors duration-300">
                                    <BookOpen className="h-6 w-6" />
                                </div>
                                <span className={`text-xs font-bold px-2 py-1 rounded-full ${
                                    cls.availableSeats > 0 ? 'bg-success-100 text-success-700' : 'bg-danger-100 text-danger-700'
                                }`}>
                                    {cls.availableSeats > 0 ? 'Available' : 'Full'}
                                </span>
                            </div>

                            <h3 className="text-xl font-bold text-academic-900 mb-4">{cls.className}</h3>

                            <div className="space-y-4 mb-6">
                                <div className="flex justify-between text-sm">
                                    <span className="text-academic-500">Capacity</span>
                                    <span className="font-semibold text-academic-800">{cls.totalSeats} Seats</span>
                                </div>
                                <div className="w-full bg-academic-100 h-2 rounded-full overflow-hidden">
                                    <div
                                        className={`h-full transition-all duration-500 ${cls.availableSeats < 5 ? 'bg-danger-500' : 'bg-academic-600'}`}
                                        style={{ width: `${((cls.totalSeats - cls.availableSeats) / cls.totalSeats) * 100}%` }}
                                    />
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-academic-500">Remaining</span>
                                    <span className={`font-bold ${cls.availableSeats < 5 ? 'text-danger-600' : 'text-academic-800'}`}>
                                        {cls.availableSeats} Seats
                                    </span>
                                </div>
                            </div>

                            <button
                                onClick={() => handleReserve(cls.id)}
                                disabled={cls.availableSeats === 0}
                                className={`w-full py-3 px-4 rounded-xl font-bold transition-all duration-300 flex items-center justify-center space-x-2 ${
                                    cls.availableSeats === 0
                                    ? 'bg-academic-100 text-academic-400 cursor-not-allowed'
                                    : 'bg-academic-800 text-white hover:bg-academic-900 shadow-lg hover:shadow-academic-200'
                                }`}
                            >
                                <span>{cls.availableSeats === 0 ? 'Fully Booked' : 'Reserve Seat'}</span>
                                {cls.availableSeats > 0 && <ArrowRight className="h-4 w-4" />}
                            </button>
                        </div>
                    </motion.div>
                ))}
            </div>
        </div>
    );
};

export default AdmissionDashboard;
