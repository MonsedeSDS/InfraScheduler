using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InfraScheduler.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PONumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EquipmentBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentBatches_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentBatches_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstallationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TechnicalNotes = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: false),
                    PDFReportPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CompletedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalEquipmentInstalled = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTechniciansInvolved = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalLaborHours = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EquipmentList = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstallationLogs_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstallationLogs_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProjectNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ModelNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Condition = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LastServiceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextServiceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CurrentLocation = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AssignedToJobId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignedToTechnicianId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_EquipmentCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "EquipmentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_Jobs_AssignedToJobId",
                        column: x => x.AssignedToJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_Technicians_AssignedToTechnicianId",
                        column: x => x.AssignedToTechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentInventoryRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<int>(type: "INTEGER", nullable: false),
                    ReservedForSite = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentInventoryRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentInventoryRecords_Equipment_EquipmentTypeId",
                        column: x => x.EquipmentTypeId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentInventoryRecords_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedQty = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceivedQty = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ShippedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InstalledDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentLines_EquipmentBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "EquipmentBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentLines_Equipment_EquipmentTypeId",
                        column: x => x.EquipmentTypeId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedQty = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobRequirements_Equipment_EquipmentTypeId",
                        column: x => x.EquipmentTypeId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobRequirements_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteEquipmentSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentQty = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdateUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteEquipmentSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteEquipmentSnapshots_Equipment_EquipmentTypeId",
                        column: x => x.EquipmentTypeId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteEquipmentSnapshots_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentDiscrepancies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedQty = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceivedQty = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentDiscrepancies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentDiscrepancies_EquipmentLines_LineId",
                        column: x => x.LineId,
                        principalTable: "EquipmentLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobTaskEquipmentLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentLineId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTaskEquipmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobTaskEquipmentLines_EquipmentLines_EquipmentLineId",
                        column: x => x.EquipmentLineId,
                        principalTable: "EquipmentLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobTaskEquipmentLines_JobTasks_JobTaskId",
                        column: x => x.JobTaskId,
                        principalTable: "JobTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteEquipmentLedgers",
                columns: table => new
                {
                    LedgerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityInstalled = table.Column<int>(type: "INTEGER", nullable: false),
                    InstallationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SourceJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    BatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    LineId = table.Column<int>(type: "INTEGER", nullable: false),
                    SerialNumbers = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteEquipmentLedgers", x => x.LedgerId);
                    table.ForeignKey(
                        name: "FK_SiteEquipmentLedgers_EquipmentBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "EquipmentBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteEquipmentLedgers_EquipmentLines_LineId",
                        column: x => x.LineId,
                        principalTable: "EquipmentLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteEquipmentLedgers_Equipment_EquipmentTypeId",
                        column: x => x.EquipmentTypeId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteEquipmentLedgers_Jobs_SourceJobId",
                        column: x => x.SourceJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteEquipmentLedgers_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "EquipmentCategories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Large construction and industrial equipment", "Heavy Machinery" },
                    { 2, "Transportation and utility vehicles", "Vehicles" },
                    { 3, "Power generation equipment", "Generators" },
                    { 4, "Telecommunications and networking equipment", "Communication Equipment" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ProjectId",
                table: "Jobs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_AssignedToJobId",
                table: "Equipment",
                column: "AssignedToJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_AssignedToTechnicianId",
                table: "Equipment",
                column: "AssignedToTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_CategoryId",
                table: "Equipment",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentBatches_JobId",
                table: "EquipmentBatches",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentBatches_SiteId",
                table: "EquipmentBatches",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentDiscrepancies_LineId",
                table: "EquipmentDiscrepancies",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentInventoryRecords_EquipmentTypeId",
                table: "EquipmentInventoryRecords",
                column: "EquipmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentInventoryRecords_SiteId",
                table: "EquipmentInventoryRecords",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentLines_BatchId",
                table: "EquipmentLines",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentLines_EquipmentTypeId",
                table: "EquipmentLines",
                column: "EquipmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallationLogs_JobId",
                table: "InstallationLogs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallationLogs_SiteId",
                table: "InstallationLogs",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRequirements_EquipmentTypeId",
                table: "JobRequirements",
                column: "EquipmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRequirements_JobId",
                table: "JobRequirements",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTaskEquipmentLines_EquipmentLineId",
                table: "JobTaskEquipmentLines",
                column: "EquipmentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTaskEquipmentLines_JobTaskId",
                table: "JobTaskEquipmentLines",
                column: "JobTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ClientId",
                table: "Projects",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteEquipmentLedgers_BatchId",
                table: "SiteEquipmentLedgers",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteEquipmentLedgers_EquipmentTypeId",
                table: "SiteEquipmentLedgers",
                column: "EquipmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteEquipmentLedgers_LineId",
                table: "SiteEquipmentLedgers",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteEquipmentLedgers_SiteId",
                table: "SiteEquipmentLedgers",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteEquipmentLedgers_SourceJobId",
                table: "SiteEquipmentLedgers",
                column: "SourceJobId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteEquipmentSnapshots_EquipmentTypeId",
                table: "SiteEquipmentSnapshots",
                column: "EquipmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteEquipmentSnapshots_SiteId",
                table: "SiteEquipmentSnapshots",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Projects_ProjectId",
                table: "Jobs",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Projects_ProjectId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "EquipmentDiscrepancies");

            migrationBuilder.DropTable(
                name: "EquipmentInventoryRecords");

            migrationBuilder.DropTable(
                name: "InstallationLogs");

            migrationBuilder.DropTable(
                name: "JobRequirements");

            migrationBuilder.DropTable(
                name: "JobTaskEquipmentLines");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "SiteEquipmentLedgers");

            migrationBuilder.DropTable(
                name: "SiteEquipmentSnapshots");

            migrationBuilder.DropTable(
                name: "EquipmentLines");

            migrationBuilder.DropTable(
                name: "EquipmentBatches");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "EquipmentCategories");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ProjectId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PONumber",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Jobs");
        }
    }
}
