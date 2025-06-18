using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InfraScheduler.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSchedulingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialRequirements_Materials_MaterialId",
                table: "MaterialRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialRequirements_Materials_MaterialId1",
                table: "MaterialRequirements");

            migrationBuilder.DropTable(
                name: "Allocations");

            migrationBuilder.DropTable(
                name: "JobTaskMaterials");

            migrationBuilder.DropTable(
                name: "MaterialDeliveries");

            migrationBuilder.DropTable(
                name: "MaterialInventory");

            migrationBuilder.DropTable(
                name: "MaterialReservations");

            migrationBuilder.DropTable(
                name: "ResourceCalendars");

            migrationBuilder.DropTable(
                name: "ScheduleSlots");

            migrationBuilder.DropTable(
                name: "MaterialResources");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "ResourceCategories");

            migrationBuilder.DropIndex(
                name: "IX_MaterialRequirements_MaterialId",
                table: "MaterialRequirements");

            migrationBuilder.DropIndex(
                name: "IX_MaterialRequirements_MaterialId1",
                table: "MaterialRequirements");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "MaterialRequirements");

            migrationBuilder.DropColumn(
                name: "MaterialId1",
                table: "MaterialRequirements");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "MaterialRequirements");

            migrationBuilder.DropColumn(
                name: "PlannedEnd",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "PlannedStart",
                table: "JobTasks");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "MaterialRequirements",
                newName: "RequiredTo");

            migrationBuilder.AddColumn<DateTime>(
                name: "RequiredFrom",
                table: "MaterialRequirements",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ToolCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_Tools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tools_Jobs_AssignedToJobId",
                        column: x => x.AssignedToJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tools_Technicians_AssignedToTechnicianId",
                        column: x => x.AssignedToTechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tools_ToolCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ToolCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ToolId = table.Column<int>(type: "INTEGER", nullable: false),
                    TechnicianId = table.Column<int>(type: "INTEGER", nullable: true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: true),
                    CheckoutDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpectedReturnDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualReturnDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolAssignments_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolAssignments_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolAssignments_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToolMaintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ToolId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaintenanceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PerformedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolMaintenance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolMaintenance_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ToolCategories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Electric and battery-powered tools", "Power Tools" },
                    { 2, "Manual tools and equipment", "Hand Tools" },
                    { 3, "Diagnostic and testing tools", "Testing Equipment" },
                    { 4, "Personal protective equipment", "Safety Equipment" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssignments_JobId",
                table: "ToolAssignments",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssignments_TechnicianId",
                table: "ToolAssignments",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssignments_ToolId",
                table: "ToolAssignments",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolMaintenance_ToolId",
                table: "ToolMaintenance",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_AssignedToJobId",
                table: "Tools",
                column: "AssignedToJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_AssignedToTechnicianId",
                table: "Tools",
                column: "AssignedToTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_CategoryId",
                table: "Tools",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToolAssignments");

            migrationBuilder.DropTable(
                name: "ToolMaintenance");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "ToolCategories");

            migrationBuilder.DropColumn(
                name: "RequiredFrom",
                table: "MaterialRequirements");

            migrationBuilder.RenameColumn(
                name: "RequiredTo",
                table: "MaterialRequirements",
                newName: "Unit");

            migrationBuilder.AddColumn<int>(
                name: "MaterialId",
                table: "MaterialRequirements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaterialId1",
                table: "MaterialRequirements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Quantity",
                table: "MaterialRequirements",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEnd",
                table: "JobTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStart",
                table: "JobTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    PartNumber = table.Column<string>(type: "TEXT", nullable: false),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    TechnicianId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduledEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduledStart = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleSlots_JobTasks_JobTaskId",
                        column: x => x.JobTaskId,
                        principalTable: "JobTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleSlots_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobTaskMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTaskMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobTaskMaterials_JobTasks_JobTaskId",
                        column: x => x.JobTaskId,
                        principalTable: "JobTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobTaskMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<double>(type: "REAL", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Supplier = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialDeliveries_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialInventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialInventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialInventory_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AvailableTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialResources_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceType = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_ResourceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ResourceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialResourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ReservedFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReservedTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialReservations_JobTasks_JobTaskId",
                        column: x => x.JobTaskId,
                        principalTable: "JobTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialReservations_MaterialResources_MaterialResourceId",
                        column: x => x.MaterialResourceId,
                        principalTable: "MaterialResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Allocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    TechnicianId = table.Column<int>(type: "INTEGER", nullable: false),
                    AllocationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoursAllocated = table.Column<int>(type: "INTEGER", nullable: false),
                    ResourceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Allocations_JobTasks_JobTaskId",
                        column: x => x.JobTaskId,
                        principalTable: "JobTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Allocations_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Allocations_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceCalendars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TechnicianId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceCalendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceCalendars_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResourceCalendars_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ResourceCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Technician" },
                    { 2, "Vehicle" },
                    { 3, "Tool" },
                    { 4, "Budget" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequirements_MaterialId",
                table: "MaterialRequirements",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequirements_MaterialId1",
                table: "MaterialRequirements",
                column: "MaterialId1");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_JobTaskId",
                table: "Allocations",
                column: "JobTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_ResourceId",
                table: "Allocations",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_TechnicianId",
                table: "Allocations",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTaskMaterials_JobTaskId",
                table: "JobTaskMaterials",
                column: "JobTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTaskMaterials_MaterialId",
                table: "JobTaskMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialDeliveries_MaterialId",
                table: "MaterialDeliveries",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInventory_MaterialId",
                table: "MaterialInventory",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReservations_JobTaskId",
                table: "MaterialReservations",
                column: "JobTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReservations_MaterialResourceId",
                table: "MaterialReservations",
                column: "MaterialResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialResources_MaterialId",
                table: "MaterialResources",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCalendars_ResourceId",
                table: "ResourceCalendars",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCalendars_TechnicianId",
                table: "ResourceCalendars",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CategoryId",
                table: "Resources",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_JobTaskId",
                table: "ScheduleSlots",
                column: "JobTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_TechnicianId",
                table: "ScheduleSlots",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialRequirements_Materials_MaterialId",
                table: "MaterialRequirements",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialRequirements_Materials_MaterialId1",
                table: "MaterialRequirements",
                column: "MaterialId1",
                principalTable: "Materials",
                principalColumn: "Id");
        }
    }
}
