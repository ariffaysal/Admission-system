import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { admissionApi } from '../api/services';
import { type AdmissionHold } from '../types';
import { Loading, ErrorMessage } from '../components/Common';
import { Timer, CheckCircle, AlertTriangle, ArrowLeft } from 'lucide-react';
import { motion } from 'framer-motion';
import toast from 'react-hot-toast';

const ReservationDetails = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [hold, setHold] = useState<AdmissionHold | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [timeLeft, setTimeLeft] = useState<number | null>(null);

    useEffect(() => {
        const fetchHold = async () => {
            try {
                const data = await admissionApi.getHold(id!);
                setHold(data);
                setTimeLeft(data.status === 'HELD' ? Math.max(0, Math.floor((new Date(data.expiresAt).getTime() - Date.now()) / 1000)) : 0);
            } catch (err: any) {
                setError(err.message || 'Failed to load reservation');
            } finally {
                setLoading(false);
            }
        };
        fetchHold();
        const refresh = setInterval(fetchHold, 5000);

        const timer = setInterval(() => {
            setTimeLeft(prev => (prev !== null && prev > 0) ? prev - 1 : 0);
        }, 1000);

        return () => { clearInterval(timer); clearInterval(refresh); };
    }, [id]);

    const formatTime = (seconds: number) => {
        const m = Math.floor(seconds / 60);
        const s = seconds % 60;
        return `${m}:${s.toString().padStart(2, '0')}`;
    };

    const getTimeColor = (seconds: number) => {
        if (seconds > 300) return 'text-success-600'; // > 5 mins
        if (seconds > 60) return 'text-warning-500'; // > 1 min
        return 'text-danger-600'; // < 1 min
    };

    const handleConfirm = async () => {
        try {
            const updated = await admissionApi.confirm(id!);
            setHold(updated);
            toast.success('Admission Confirmed Successfully!');
            navigate('/my-applications');
        } catch (err: any) {
            toast.error(err.response?.data?.error || 'Confirmation failed');
        }
    };

    const handleCancel = async () => {
        if (!window.confirm('Are you sure you want to cancel this reservation?')) return;
        try {
            await admissionApi.cancel(id!);
            toast.success('Reservation cancelled');
            navigate('/my-applications');
        } catch (err: any) {
            toast.error(err.response?.data?.error || 'Cancellation failed');
        }
    };

    if (loading) return <Loading />;
    if (error) return <ErrorMessage message={error} />;
    if (!hold) return <ErrorMessage message="Reservation not found" />;

    const isExpired = hold.status === 'EXPIRED' || (hold.status === 'HELD' && timeLeft === 0);
    const isConfirmed = hold.status === 'CONFIRMED';

    return (
        <div className="max-w-2xl mx-auto space-y-8">
            <button
                onClick={() => navigate(-1)}
                className="flex items-center text-academic-500 hover:text-academic-800 transition-colors group"
            >
                <ArrowLeft className="h-4 w-4 mr-2 group-hover:-translate-x-1 transition-transform" />
                Back to Dashboard
            </button>

            <motion.div
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                className="bg-white rounded-3xl shadow-xl border border-academic-200 overflow-hidden"
            >
                <div className={`p-8 text-center ${isConfirmed ? 'bg-success-500 text-white' : 'bg-academic-800 text-white'}`}>
                    <div className="inline-flex p-3 bg-white/20 rounded-full mb-4">
                        {isConfirmed ? <CheckCircle className="h-8 w-8" /> : <Timer className="h-8 w-8" />}
                    </div>
                    <h2 className="text-3xl font-bold mb-2">
                        {isConfirmed ? 'Admission Confirmed!' : hold.status === 'HELD' ? 'Seat Reserved' : `Reservation ${hold.status.toLowerCase()}`}
                    </h2>
                    <p className="opacity-90">
                        {isConfirmed
                            ? 'Your child is now officially enrolled.'
                            : 'Please confirm your admission within the time limit.'}
                    </p>
                </div>

                <div className="p-8 space-y-6">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                        <div className="p-4 bg-academic-50 rounded-2xl border border-academic-100">
                            <p className="text-xs text-academic-500 uppercase font-bold mb-1">Reservation ID</p>
                            <p className="font-mono text-sm font-semibold">{hold.holdId}</p>
                        </div>
                        <div className="p-4 bg-academic-50 rounded-2xl border border-academic-100">
                            <p className="text-xs text-academic-500 uppercase font-bold mb-1">Status</p>
                            <p className={`font-bold ${isConfirmed ? 'text-success-600' : isExpired ? 'text-danger-600' : 'text-blue-600'}`}>
                                {isExpired ? 'EXPIRED' : hold.status}
                            </p>
                        </div>
                    </div>

                    {hold.status === 'HELD' && (
                        <div className="flex flex-col items-center justify-center p-6 bg-academic-100 rounded-2xl border-2 border-dashed border-academic-300">
                            <p className="text-sm text-academic-500 mb-2">Reservation Expires In</p>
                            <p className={`text-5xl font-black font-mono ${getTimeColor(timeLeft || 0)}`}>
                                {formatTime(timeLeft || 0)}
                            </p>
                            {isExpired && (
                                <div className="mt-4 flex items-center text-danger-600 font-bold">
                                    <AlertTriangle className="h-4 w-4 mr-2" />
                                    This reservation has expired
                                </div>
                            )}
                        </div>
                    )}

                    <div className="flex flex-col sm:flex-row gap-4 pt-4">
                        {hold.status === 'HELD' && !isExpired && (
                            <button
                                onClick={handleConfirm}
                                className="flex-1 bg-success-600 text-white py-4 rounded-xl font-bold text-lg hover:bg-success-700 transition-all shadow-lg hover:shadow-success-200 active:scale-95"
                            >
                                Confirm Admission (ভর্তি ফি দিন)
                            </button>
                        )}
                        <button
                            onClick={handleCancel}
                            disabled={hold.status !== 'HELD'}
                            className="flex-1 bg-white text-gray-600 border border-gray-300 py-4 rounded-xl font-bold hover:bg-gray-50 transition-all disabled:opacity-50"
                        >
                            Cancel Reservation
                        </button>
                    </div>
                </div>
            </motion.div>
        </div>
    );
};

export default ReservationDetails;
