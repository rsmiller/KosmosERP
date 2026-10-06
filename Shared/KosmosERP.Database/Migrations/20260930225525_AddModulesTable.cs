using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace KosmosERP.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddModulesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "modules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    module_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    module_name = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<string>(type: "longtext", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<string>(type: "longtext", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modules", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 20, 26, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "modules");

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937), new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937), new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937), new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937), new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937), new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937) });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937), new DateTime(2026, 9, 18, 15, 20, 26, 312, DateTimeKind.Utc).AddTicks(937) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978), new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978), new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978), new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978), new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978), new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978), new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978), new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978), new DateTime(2026, 9, 18, 15, 20, 26, 292, DateTimeKind.Utc).AddTicks(1978) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450), new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450), new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450), new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450), new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450), new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450), new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450), new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450) });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_on", "updated_on" },
                values: new object[] { new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450), new DateTime(2026, 9, 18, 15, 20, 26, 310, DateTimeKind.Utc).AddTicks(3450) });
        }
    }
}
