export interface ClassOffering {
    id: string;
    name: string;
    totalSeats: number;
    heldSeats: number;
    confirmedSeats: number;
    availableSeats: number;
}

export interface AdmissionHold {
    holdId: string;
    studentId: string;
    classId: string;
    className: string;
    expiresAt: string;
    status: 'HELD' | 'CONFIRMED' | 'EXPIRED' | 'CANCELLED';
}

export interface CreateClassRequest {
    name: string;
    totalSeats: number;
}
