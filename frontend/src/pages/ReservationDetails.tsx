import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { admissionApi } from '../api/services';
import { AdmissionHold } from '../types';
import { Loading, ErrorMessage, Notification } from '../components/Common';

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
                const expiry = new Date(data.expiresAt).getTime();
                setTimeLeft(Math.max(0, Math.floor((expiry - Date.now()) / 1000)));
            } catch (err: any) {
                setError(err.message || 'Failed to load reservation');
            } finally {
                setLoading(false);
            }
        };
        fetchHold();

        const timer = setInterval(() => {
            setTimeLeft(prev => (prev !== null && prev > 0) ? prev - 1 : 0);
        }, 1000);

        return () => clearInterval(timer);
    }, [id]);

    const formatTime = (seconds: number) => {
        const m = Math.floor(seconds / 60);
        const s = seconds % 60;
        return `${m}:${s.toString().padStart(2, '0')}`;
    };

    const handleConfirm = async () => {
        try {
            await admissionApi.confirm(id!);
            alert('Admission Confirmed Successfully!');
            navigate('/my-applications');
        } catch (err: any) {
            alert(err.message || 'Confirmation failed');
        }
    };

    const handleCancel = async () => {
        if (!window.confirm('Are you sure you want to cancel this reservation?')) return;
        try {
            await admissionApi.cancel(id!);
            alert('Reservation Cancelled');
            navigate('/my-applications');
        } catch (err: any) {
            alert(err.message || 'Cancellation failed');
        }
    };

    if (loading) return <Loading />;
    if (error) return <ErrorMessage message={error} />;
    if (!hold) return <ErrorMessage message="Reservation not found" />;

    const isExpired = timeLeft === 0;

    return (
        <div className="max-w-lg mx-auto bg-white p-8 rounded-lg shadow-md border border-gray-200">
            <h2 className="text-2xl font-bold mb-6">Reservation Details</h2>
            <div className="space-y-4 mb-8">
                <div className="flex justify-between border-b py-2">
                    <span className="text-gray-600">Reservation ID:</span>
                    <span className="font-mono">{hold.id}</span>
                </div>
                <div className="flex justify-between border-b py-2">
                    <span className="text-gray-600">Status:</span>
                    <span className={`font-bold ${hold.isConfirmed ? 'text-green-600' : isExpired ? 'text-red-600' : 'text-blue-600'}`}>
                        {hold.isConfirmed ? 'CONFIRMED' : isExpired ? 'EXPIRED' : 'HELD'}
                    </span>
                </div>
                {!hold.isConfirmed && (
                    <div className="flex justify-between border-b py-2">
                        <span className="text-gray-600">Expires In:</span>
                        <span className={`font-mono font-bold ${isExpired ? 'text-red-600' : 'text-gray-900'}`}>
                            {formatTime(timeLeft || 0)}
                        </span>
                    </div>
                )}
            </div>
            <div className="flex space-x-4">
                {!hold.isConfirmed && !isExpired && (
                    <button
                        onClick={handleConfirm}
                        className="flex-1 bg-green-600 text-white py-3 rounded-lg font-bold hover:bg-green-700 transition-colors"
                    >
                        Confirm Admission (ভর্তি ফি দিন)
                    </button>
                )}
                <button
                    onClick={handleCancel}
                    disabled={hold.isConfirmed}
                    className="flex-1 bg-gray-200 text-gray-700 py-3 rounded-lg font-bold hover:bg-gray-300 transition-colors disabled:opacity-50"
                >
                    Cancel Reservation
                </button>
            </div>
        </div>
    );
};

export default ReservationDetails;
