using System;
using System.Collections.Generic;

namespace AdmissionSystem.Api.Entities
{
    public class ClassOffering
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual ICollection<AdmissionHold> Holds { get; set; } = new List<AdmissionHold>();
    }

    public class AdmissionHold
    {
        public Guid Id { get; set; }
        public Guid ClassOfferingId { get; set; }
        public virtual ClassOffering ClassOffering { get; set; } = null!;
        public string StudentId { get; set; } = string.Empty;
        public string Status { get; set; } = "HELD";
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
