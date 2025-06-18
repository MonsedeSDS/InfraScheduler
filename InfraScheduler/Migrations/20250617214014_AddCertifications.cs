using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InfraScheduler.Migrations
{
    /// <inheritdoc />
    public partial class AddCertifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_SiteOwners_SiteOwnerId",
                table: "Sites");

            migrationBuilder.AddColumn<int>(
                name: "SiteOwnerId1",
                table: "Sites",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SiteOwners",
                columns: new[] { "Id", "Address", "CompanyName", "ContactPerson", "Email", "Phone" },
                values: new object[,]
                {
                    { 1, "Cotonou, Benin", "Prime Infrastructure & Engineering", "John Doe", "contact@primeinfra.bj", "+229 12345678" },
                    { 2, "Porto-Novo, Benin", "Benin Telecom", "Jane Smith", "contact@benintelecom.bj", "+229 87654321" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sites_SiteOwnerId1",
                table: "Sites",
                column: "SiteOwnerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_SiteOwners_SiteOwnerId",
                table: "Sites",
                column: "SiteOwnerId",
                principalTable: "SiteOwners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_SiteOwners_SiteOwnerId1",
                table: "Sites",
                column: "SiteOwnerId1",
                principalTable: "SiteOwners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_SiteOwners_SiteOwnerId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_SiteOwners_SiteOwnerId1",
                table: "Sites");

            migrationBuilder.DropTable(
                name: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_Sites_SiteOwnerId1",
                table: "Sites");

            migrationBuilder.DeleteData(
                table: "SiteOwners",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SiteOwners",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "SiteOwnerId1",
                table: "Sites");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_SiteOwners_SiteOwnerId",
                table: "Sites",
                column: "SiteOwnerId",
                principalTable: "SiteOwners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
