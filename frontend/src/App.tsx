import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import AdmissionDashboard from './pages/AdmissionDashboard';
import StudentIdPage from './pages/StudentIdPage';
import MyApplications from './pages/MyApplications';
import ReservationDetails from './pages/ReservationDetails';
import AdminDashboard from './pages/AdminDashboard';
import CreateClass from './pages/CreateClass';

const App = () => {
    return (
        <BrowserRouter>
            <div className="min-h-screen bg-gray-50 text-gray-900">
                <nav className="bg-blue-700 text-white p-4 shadow-md flex justify-between items-center">
                    <h1 className="text-xl font-bold">School Admission System</h1>
                    <div className="space-x-4">
                        <a href="/" className="hover:underline">Dashboard</a>
                        <a href="/my-applications" className="hover:underline">My Applications</a>
                        <a href="/admin" className="hover:underline">Admin</a>
                    </div>
                </nav>

                <main className="container mx-auto p-4">
                    <Routes>
                        <Route path="/" element={<AdmissionDashboard />} />
                        <Route path="/student-id" element={<StudentIdPage />} />
                        <Route path="/my-applications" element={<MyApplications />} />
                        <Route path="/reservation/:id" element={<ReservationDetails />} />
                        <Route path="/admin" element={<AdminDashboard />} />
                        <Route path="/admin/create-class" element={<CreateClass />} />
                        <Route path="*" element={<Navigate to="/" />} />
                    </Routes>
                </main>
            </div>
        </BrowserRouter>
    );
};

export default App;
