using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KosmosERP.Database.Migrations
{
    /// <inheritdoc />
    public partial class RetireOldProductionStatuses : Migration
    {
        // Production statuses lived in two lookup lists. The API, the seeder and the shipping
        // "Ready To Ship" list use production_order_status_* under module 97dd4b13-... , but
        // the UI dropdowns and sales-order-created production orders used the older
        // production_status_* list under module f157469e-... (BUG-003). Move every production
        // order header and line onto the new keys, by stage, then retire the old list.
        // The new list's entries (including the added Canceled) are created at API startup by
        // ProductionOrderModule.SeedPermissions.
        private static readonly (string from, string to)[] StatusMap =
        {
            ("production_status_new", "production_order_status_submitted"),
            ("production_status_released", "production_order_status_submitted"),
            ("production_status_scheduled", "production_order_status_submitted"),
            ("production_status_picking", "production_order_status_pulled"),
            ("production_status_production", "production_order_status_wip"),
            ("production_status_qc", "production_order_status_qc"),
            ("production_status_completed", "production_order_status_complete"),
            ("production_status_canceled", "production_order_status_canceled"),
        };

        private const string OldModuleId = "f157469e-5e5c-4a5b-b071-89a28b2a0310";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in new[] { "production_order_headers", "production_order_lines" })
            {
                foreach (var (from, to) in StatusMap)
                {
                    migrationBuilder.Sql($"UPDATE {table} SET status = '{to}' WHERE status = '{from}';");
                }
            }

            // Soft delete, like the API does, so the old entries stay for history but drop out
            // of Admin > Lists.
            migrationBuilder.Sql(
                "UPDATE key_value_stores " +
                "SET is_deleted = 1, deleted_on = UTC_TIMESTAMP(), deleted_on_string = DATE_FORMAT(UTC_TIMESTAMP(), '%Y-%m-%d %H:%i:%sZ'), deleted_on_timezone = '+00:00' " +
                $"WHERE module_id = '{OldModuleId}' AND is_deleted = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the old list's entries. Order statuses aren't mapped back: New, Released
            // and Scheduled all became Submitted, so the original stage can't be recovered.
            migrationBuilder.Sql(
                "UPDATE key_value_stores " +
                "SET is_deleted = 0, deleted_on = NULL, deleted_on_string = NULL, deleted_on_timezone = NULL " +
                $"WHERE module_id = '{OldModuleId}';");
        }
    }
}
