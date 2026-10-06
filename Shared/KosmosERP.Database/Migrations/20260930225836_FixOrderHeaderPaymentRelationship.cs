using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace KosmosERP.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderHeaderPaymentRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_headers_payments_id",
                table: "order_headers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_payments_order_header_id",
                table: "payments");

            // order_headers.id is referenced by other tables' foreign keys (order_lines, ...), and
            // MySQL refuses to MODIFY a referenced column while FK checks are on. The column
            // keeps its type and values; only AUTO_INCREMENT changes, so this is safe.
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 0;");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "order_headers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 1;");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_order_headers_order_header_id",
                table: "payments",
                column: "order_header_id",
                principalTable: "order_headers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_order_headers_order_header_id",
                table: "payments");

            // order_headers.id is referenced by other tables' foreign keys (order_lines, ...), and
            // MySQL refuses to MODIFY a referenced column while FK checks are on. The column
            // keeps its type and values; only AUTO_INCREMENT changes, so this is safe.
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 0;");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "order_headers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 1;");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_payments_order_header_id",
                table: "payments",
                column: "order_header_id");

            migrationBuilder.AddForeignKey(
                name: "FK_order_headers_payments_id",
                table: "order_headers",
                column: "id",
                principalTable: "payments",
                principalColumn: "order_header_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
