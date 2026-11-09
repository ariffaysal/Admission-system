export interface ClassOffering {
    id: string;
    className: string;
    totalSeats: number;
    availableSeats: number;
}

export interface AdmissionHold {
    id: string;
    studentId: string;
    classOfferingId: string;
    expiresAt: string;
    isConfirmed: boolean;
}

export interface CreateClassRequest {
    className: string;
    totalSeats: number;
}
