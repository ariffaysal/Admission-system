import { useState } from 'react';
import { useNavigate } from 'react-router-dom';


const StudentIdPage = () => {
    const [studentId, setStudentId] = useState(localStorage.getItem('student_id') ?? '');
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const handleSave = (e: React.FormEvent) => {
        e.preventDefault();
        const normalized = studentId.trim();
        if (!/^\d{5,50}$/.test(normalized)) return;

        setLoading(true);
        localStorage.setItem('student_id', normalized);
        setTimeout(() => {
            setLoading(false);
            navigate('/');
        }, 500);
    };

    return (
        <div className="max-w-md mx-auto mt-10 bg-white p-8 rounded-lg shadow-lg">
            <h2 className="text-2xl font-bold mb-6 text-center">Student Identification</h2>
            <form onSubmit={handleSave} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Student ID</label>
                    <input
                        type="text"
                        value={studentId}
                        onChange={(e) => setStudentId(e.target.value)}
                        className="w-full p-2 border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 outline-none"
                        placeholder="Enter your student ID"
                        required
                        minLength={5}
                        maxLength={50}
                        pattern="[0-9]{5,50}"
                        title="Enter 5 to 50 digits"
                    />
                </div>
                <button
                    type="submit"
                    disabled={loading}
                    className="w-full bg-blue-600 text-white py-2 px-4 rounded font-semibold hover:bg-blue-700 disabled:bg-blue-300 transition-colors"
                >
                    {loading ? 'Saving...' : 'Continue'}
                </button>
            </form>
        </div>
    );
};

export default StudentIdPage;
