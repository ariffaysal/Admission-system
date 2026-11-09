using System;

namespace AdmissionSystem.Api.DTOs
{
    public record CreateClassRequest(string ClassName, int TotalSeats);

    public record ClassOfferingDto(
        Guid Id,
        string ClassName,
        int TotalSeats,
        int AvailableSeats
    );

    public record AdmissionHoldDto(
        Guid Id,
        string StudentId,
        Guid ClassOfferingId,
        DateTimeOffset ExpiresAt,
        bool IsConfirmed
    );
}
