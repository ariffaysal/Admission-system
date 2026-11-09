using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdmissionSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassOfferings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    AvailableSeats = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassOfferings", x => x.Id);
                    table.CheckConstraint("CK_ClassOffering_TotalSeats_Positive", "\"TotalSeats\" > 0");
                    table.CheckConstraint("CK_ClassOffering_AvailableSeats_NonNegative", "\"AvailableSeats\" >= 0");
                    table.CheckConstraint("CK_ClassOffering_SeatLimit", "\"AvailableSeats\" <= \"TotalSeats\"");
                });

            migrationBuilder.CreateTable(
                name: "AdmissionHolds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassOfferingId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "HELD"),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissionHolds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdmissionHolds_ClassOfferings_ClassOfferingId",
                        column: x => x.ClassOfferingId,
                        principalTable: "ClassOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionHolds_ClassOfferingId",
                table: "AdmissionHolds",
                column: "ClassOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionHolds_ExpiresAt",
                table: "AdmissionHolds",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionHolds_StudentId",
                table: "AdmissionHolds",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionHolds_StudentId_ClassOfferingId",
                table: "AdmissionHolds",
                columns: new[] { "StudentId", "ClassOfferingId" },
                unique: true,
                filter: "\"Status\" IN ('HELD', 'CONFIRMED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdmissionHolds");

            migrationBuilder.DropTable(
                name: "ClassOfferings");
        }
    }
}
