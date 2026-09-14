using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace KosmosERP.Database.Migrations
{
    /// <inheritdoc />
    public partial class _04052026moreaccounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chart_of_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    account_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    account_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    account_type = table.Column<int>(type: "int", nullable: false),
                    parent_account_id = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    normal_balance = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
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
                    table.PrimaryKey("PK_chart_of_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_chart_of_accounts_chart_of_accounts_parent_account_id",
                        column: x => x.parent_account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "journal_entry_headers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    entry_number = table.Column<int>(type: "int", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    reference_type = table.Column<int>(type: "int", nullable: true),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    is_posted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    posted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    is_reversed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    reversed_by_entry_id = table.Column<int>(type: "int", nullable: true),
                    fiscal_period = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
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
                    table.PrimaryKey("PK_journal_entry_headers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "financial_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    transaction_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    transaction_type = table.Column<int>(type: "int", nullable: false),
                    source_module = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    source_id = table.Column<int>(type: "int", nullable: false),
                    source_guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    chart_of_account_id = table.Column<int>(type: "int", nullable: false),
                    debit_amount = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    credit_amount = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    running_balance = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    fiscal_period = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    journal_entry_id = table.Column<int>(type: "int", nullable: true),
                    is_reversal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
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
                    table.PrimaryKey("PK_financial_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_financial_transactions_chart_of_accounts_chart_of_account_id",
                        column: x => x.chart_of_account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "journal_entry_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    journal_entry_header_id = table.Column<int>(type: "int", nullable: false),
                    line_number = table.Column<int>(type: "int", nullable: false),
                    chart_of_account_id = table.Column<int>(type: "int", nullable: false),
                    debit_amount = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    credit_amount = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
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
                    table.PrimaryKey("PK_journal_entry_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_journal_entry_lines_chart_of_accounts_chart_of_account_id",
                        column: x => x.chart_of_account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_journal_entry_lines_journal_entry_headers_journal_entry_head~",
                        column: x => x.journal_entry_header_id,
                        principalTable: "journal_entry_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 491, DateTimeKind.Utc).AddTicks(6989), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 474, DateTimeKind.Utc).AddTicks(568), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00", new DateTime(2026, 4, 5, 15, 49, 18, 490, DateTimeKind.Utc).AddTicks(4900), "2026-04-05 15:49:18Z", "-05:00" });

            migrationBuilder.CreateIndex(
                name: "IX_chart_of_accounts_account_number",
                table: "chart_of_accounts",
                column: "account_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chart_of_accounts_account_type",
                table: "chart_of_accounts",
                column: "account_type");

            migrationBuilder.CreateIndex(
                name: "IX_chart_of_accounts_guid",
                table: "chart_of_accounts",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_chart_of_accounts_is_active",
                table: "chart_of_accounts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_chart_of_accounts_parent_account_id",
                table: "chart_of_accounts",
                column: "parent_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_chart_of_account_id",
                table: "financial_transactions",
                column: "chart_of_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_fiscal_period",
                table: "financial_transactions",
                column: "fiscal_period");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_guid",
                table: "financial_transactions",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_journal_entry_id",
                table: "financial_transactions",
                column: "journal_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_source_id",
                table: "financial_transactions",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_source_module",
                table: "financial_transactions",
                column: "source_module");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_transaction_date",
                table: "financial_transactions",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_transaction_type",
                table: "financial_transactions",
                column: "transaction_type");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_headers_entry_date",
                table: "journal_entry_headers",
                column: "entry_date");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_headers_entry_number",
                table: "journal_entry_headers",
                column: "entry_number");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_headers_fiscal_period",
                table: "journal_entry_headers",
                column: "fiscal_period");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_headers_guid",
                table: "journal_entry_headers",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_headers_is_posted",
                table: "journal_entry_headers",
                column: "is_posted");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_headers_reference_id",
                table: "journal_entry_headers",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_headers_reference_type",
                table: "journal_entry_headers",
                column: "reference_type");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_chart_of_account_id",
                table: "journal_entry_lines",
                column: "chart_of_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_guid",
                table: "journal_entry_lines",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_journal_entry_header_id",
                table: "journal_entry_lines",
                column: "journal_entry_header_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "financial_transactions");

            migrationBuilder.DropTable(
                name: "journal_entry_lines");

            migrationBuilder.DropTable(
                name: "chart_of_accounts");

            migrationBuilder.DropTable(
                name: "journal_entry_headers");

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 606, DateTimeKind.Utc).AddTicks(4906), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 589, DateTimeKind.Utc).AddTicks(6026), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_on", "created_on_string", "created_on_timezone", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[] { new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00", new DateTime(2026, 1, 1, 19, 46, 24, 605, DateTimeKind.Utc).AddTicks(3568), "2026-01-01 19:46:24Z", "-06:00" });
        }
    }
}
