using System;

namespace AdmissionSystem.Api.DTOs
{
    public record CreateClassRequest(string Name, int TotalSeats);

    public record ClassOfferingDto(
        Guid Id,
        string Name,
        int TotalSeats,
        int HeldSeats,
        int ConfirmedSeats,
        int AvailableSeats
    );

    public record AdmissionHoldDto(
        Guid HoldId,
        string StudentId,
        Guid ClassId,
        DateTimeOffset ExpiresAt,
        string Status,
        string ClassName
    );
}
