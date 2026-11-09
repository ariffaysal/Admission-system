# Assumptions & Implementation Decisions

This document outlines the design decisions and assumptions made during the development of the School Admission System.

## 1. Admin Access
The system implements a full Admin UI. Since the requirements did not explicitly specify an authentication system for administrators, admin pages are accessible via direct URL (`/admin`). In a production environment, these would be protected by a Role-Based Access Control (RBAC) system.

## 2. Student Identification
Identification is handled via the `X-Student-Id` HTTP header. 
- The frontend stores this ID in `localStorage`.
- The backend treats this header as the source of truth for student identity.
- This simplifies the process for the assignment while demonstrating how to implement identity-based requests.

## 3. Cancellation Behavior
- **Cancellable**: Reservations in the `HELD` state can be cancelled by the student, which immediately releases the seat back to the class's available-seat count.
- **Non-Cancellable**: Once a reservation is `CONFIRMED`, it cannot be cancelled through the standard cancellation API. This ensures that confirmed admissions are final.

## 4. Reservation Expiry Logic
- A hosted backend worker checks for expired reservations every five seconds.
- API reads and reservation attempts also perform just-in-time expiry so that a delayed worker does not leave stale availability visible or permit late confirmation.
- Expired reservations remain in history with `EXPIRED` status; a partial unique index allows a student to apply again after expiry.

## 5. Payment Simulation
Payment is simulated. The "ভর্তি ফি দিন" (Pay Admission Fee) button triggers the `/confirm` endpoint. If the request is successful, the system assumes payment was completed and transitions the hold to `CONFIRMED`.

## 6. Class/Shift Representation
Classes are represented as a single entity `ClassOffering`. Its `name` response field stores both the grade and shift (e.g., "Class 6 - Morning Shift").

## 7. Concurrency Control
To prevent overbooking (the "Double Booking" problem), the system uses **Pessimistic Locking** (`SELECT FOR UPDATE`). This ensures that only one transaction can modify the seat count for a specific class at a time, maintaining strict data integrity even under heavy load.
