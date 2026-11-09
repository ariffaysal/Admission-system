import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import AdmissionDashboard from './pages/AdmissionDashboard';
import StudentIdPage from './pages/StudentIdPage';
import MyApplications from './pages/MyApplications';
import ReservationDetails from './pages/ReservationDetails';
import AdminDashboard from './pages/AdminDashboard';
import CreateClass from './pages/CreateClass';
import { Toaster } from 'react-hot-toast';
import { LayoutDashboard, UserCircle, Settings, School } from 'lucide-react';

const App = () => {
    return (
        <BrowserRouter>
            <div className="min-h-screen bg-academic-50 text-academic-900 font-sans">
                <Toaster position="top-right" />

                <nav className="bg-academic-900 text-white p-4 shadow-lg sticky top-0 z-50">
                    <div className="container mx-auto flex justify-between items-center">
                        <div className="flex items-center space-x-2">
                            <School className="h-6 w-6 text-blue-400" />
                            <h1 className="text-xl font-bold tracking-tight">School Admission</h1>
                        </div>
                        <div className="hidden md:flex items-center space-x-6 text-sm font-medium">
                            <a href="/" className="hover:text-blue-400 transition-colors flex items-center space-x-1">
                                <LayoutDashboard className="h-4 w-4" />
                                <span>Dashboard</span>
                            </a>
                            <a href="/my-applications" className="hover:text-blue-400 transition-colors flex items-center space-x-1">
                                <UserCircle className="h-4 w-4" />
                                <span>My Applications</span>
                            </a>
                            <a href="/admin" className="hover:text-blue-400 transition-colors flex items-center space-x-1">
                                <Settings className="h-4 w-4" />
                                <span>Admin</span>
                            </a>
                        </div>
                    </div>
                </nav>

                <main className="container mx-auto p-6">
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
