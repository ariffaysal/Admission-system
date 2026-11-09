import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { classApi } from '../api/services';

const CreateClass = () => {
    const [name, setName] = useState('');
    const [totalSeats, setTotalSeats] = useState('');
    const [saving, setSaving] = useState(false);
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSaving(true);
        try {
            await classApi.create({ name: name.trim(), totalSeats: Number(totalSeats) });
            toast.success('Class created');
            navigate('/admin');
        } catch (error: any) {
            toast.error(error.response?.data?.error ?? 'Could not create class');
        } finally { setSaving(false); }
    };

    return <div className="max-w-2xl mx-auto p-6 bg-white rounded-xl shadow-md">
        <h2 className="text-2xl font-bold mb-6 text-academic-900">Create New Class</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
            <label className="block text-sm font-medium text-gray-700">Class and shift name
                <input value={name} onChange={e => setName(e.target.value)} maxLength={255} required placeholder="Class 6 - Morning Shift" className="mt-1 w-full p-2 border border-gray-300 rounded-md" />
            </label>
            <label className="block text-sm font-medium text-gray-700">Total seats
                <input type="number" min={1} value={totalSeats} onChange={e => setTotalSeats(e.target.value)} required className="mt-1 w-full p-2 border border-gray-300 rounded-md" />
            </label>
            <button disabled={saving} className="w-full bg-academic-900 text-white py-2 px-4 rounded-md disabled:opacity-60">{saving ? 'Creating…' : 'Create Class'}</button>
        </form>
    </div>;
};

export default CreateClass;
