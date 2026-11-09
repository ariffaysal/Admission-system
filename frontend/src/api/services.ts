import api from './axios';
import { type ClassOffering, type CreateClassRequest, type AdmissionHold } from '../types';

export const classApi = {
    getAll: async () => {
        const { data } = await api.get<ClassOffering[]>('/classes');
        return data;
    },
    getById: async (id: string) => {
        const { data } = await api.get<ClassOffering>(`/classes/${id}`);
        return data;
    },
    create: async (request: CreateClassRequest) => {
        const { data } = await api.post<ClassOffering>('/classes', request);
        return data;
    },
};

export const admissionApi = {
    createHold: async (classId: string) => {
        const { data } = await api.post<AdmissionHold>(`/classes/${classId}/holds`);
        return data;
    },
    getHold: async (holdId: string) => {
        const { data } = await api.get<AdmissionHold>(`/holds/${holdId}`);
        return data;
    },
    confirm: async (holdId: string) => {
        const { data } = await api.post<AdmissionHold>(`/holds/${holdId}/confirm`);
        return data;
    },
    cancel: async (holdId: string) => {
        await api.delete(`/holds/${holdId}`);
    },
    getMyHolds: async () => {
        const { data } = await api.get<AdmissionHold[]>(`/me/holds`);
        return data;
    },
};
