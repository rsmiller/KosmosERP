using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KosmosERP.Database.Migrations
{
    /// <inheritdoc />
    public partial class SetPaymentTermDays : Migration
    {
        // Payment terms store their length in days in int_value (the AR invoice page uses it
        // for the due date). The API used to store a sort order (1-4) there, and the dev seeder
        // left it NULL, so set the four default terms explicitly. Custom terms are untouched;
        // admins set their days on Admin > Lists.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var days in new[] { 15, 30, 45, 60 })
            {
                migrationBuilder.Sql(
                    $"UPDATE key_value_stores SET int_value = {days} " +
                    $"WHERE module_id = '93bf02ec-5578-4aa4-a45b-f82962adf4bd' AND `key` = 'payment_terms_net_{days}';");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data fix only: the previous values (a sort order or NULL) aren't worth restoring.
        }
    }
}
