using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KosmosERP.Database.Migrations
{
    /// <inheritdoc />
    public partial class AuditUserGuids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Audit columns used to hold the user's id (or "1" for system writes); they now hold
            // the user's guid. Rewrite every value that is a known users.id to that user's guid.
            // Values that don't match a user (old external ids, nulls) are left as they are.
            // The comparison is numeric so it works whatever the columns' collations are.
            foreach (var (table, column) in AuditColumns())
            {
                migrationBuilder.Sql(
                    $"UPDATE `{table}` AS x JOIN `users` AS u ON x.`{column}` REGEXP '^[0-9]+$' AND CAST(x.`{column}` AS UNSIGNED) = u.`id` " +
                    $"SET x.`{column}` = u.`guid`;");
            }

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "6b6f736d-6f73-4000-8000-000000000001", "6b6f736d-6f73-4000-8000-000000000001" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse of Up: user guids back to user ids.
            foreach (var (table, column) in AuditColumns())
            {
                migrationBuilder.Sql(
                    $"UPDATE `{table}` AS x JOIN `users` AS u ON x.`{column}` = u.`guid` " +
                    $"SET x.`{column}` = CAST(u.`id` AS CHAR);");
            }

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });

            migrationBuilder.UpdateData(
                table: "document_uploads_object_categories",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_by", "updated_by" },
                values: new object[] { "1", "1" });
        }

        // Tables with BaseDatabaseModel audit columns (from the model snapshot at this migration),
        // plus the posted_by columns, which also hold the acting user.
        private static readonly string[] AuditedTables =
        {
            "activities", "addresses", "ap_invoice_headers", "ap_invoice_lines",
            "ar_invoice_headers", "ar_invoice_lines", "boms", "chart_of_accounts",
            "comments", "contacts", "countries", "credit_memo_headers",
            "credit_memo_lines", "customer_addresses", "customers", "document_uploads",
            "document_uploads_categories", "document_uploads_object", "document_uploads_object_categories", "document_uploads_revisions",
            "document_uploads_revisions_tag", "financial_transactions", "journal_entry_headers", "journal_entry_lines",
            "key_value_stores", "leads", "message_queue", "module_permissions",
            "modules", "notifications", "opportunities", "opportunity_lines",
            "order_headers", "order_line_attributes", "order_lines", "payments",
            "product_attributes", "production_order_headers", "production_order_lines", "products",
            "purchase_order_headers", "purchase_order_lines", "purchase_order_receive_headers", "purchase_order_receive_lines",
            "purchase_order_receive_uploads", "role_permissions", "roles", "settings",
            "shipment_headers", "shipment_lines", "states", "subscription_entries",
            "subscriptions", "transactions", "user_roles", "users",
            "vendors",
        };

        private static readonly string[] PostedByTables = { "ap_invoice_headers", "ar_invoice_headers", "journal_entry_headers" };

        private static IEnumerable<(string Table, string Column)> AuditColumns()
        {
            foreach (var table in AuditedTables)
                foreach (var column in new[] { "created_by", "updated_by", "deleted_by" })
                    yield return (table, column);

            foreach (var table in PostedByTables)
                yield return (table, "posted_by");
        }
    }
}
