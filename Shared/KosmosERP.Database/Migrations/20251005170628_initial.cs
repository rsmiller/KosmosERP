using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KosmosERP.Database.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    subject = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: false),
                    activity_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    priority = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: true),
                    contact_id = table.Column<int>(type: "int", nullable: true),
                    opportunity_id = table.Column<int>(type: "int", nullable: true),
                    lead_id = table.Column<int>(type: "int", nullable: true),
                    related_entity_id = table.Column<string>(type: "longtext", nullable: true),
                    related_entity_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    is_all_day = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    location = table.Column<string>(type: "longtext", nullable: true),
                    reminder_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    reminder_time = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activities", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    street_address1 = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    street_address2 = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true),
                    city = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false),
                    state = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    postal_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    country = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ap_invoice_headers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    vendor_id = table.Column<int>(type: "int", nullable: false),
                    invoice_number = table.Column<string>(type: "varchar(255)", nullable: false),
                    invoice_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    invoice_due_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    invoice_received_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    invoice_total = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    purchase_order_receive_id = table.Column<int>(type: "int", nullable: true),
                    association_object_id = table.Column<int>(type: "int", nullable: true),
                    association_is_purchase_order = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    association_is_sales_order = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    association_is_ar_invoice = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    packing_list_is_required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_paid = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ap_invoice_headers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ar_invoice_headers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    order_header_id = table.Column<int>(type: "int", nullable: false),
                    invoice_number = table.Column<int>(type: "int", nullable: false, defaultValue: 10000),
                    payment_terms = table.Column<string>(type: "longtext", nullable: false),
                    invoice_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    invoice_due_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    invoice_total = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    tax_percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_paid = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_taxable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    paid_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ar_invoice_headers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    object_guid = table.Column<string>(type: "varchar(255)", nullable: false),
                    comment_text = table.Column<string>(type: "longtext", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "contacts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    cell_phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacts", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    country_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    iso3 = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    phonecode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    currency = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: true),
                    currency_symbol = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    region = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CreditMemoHeaders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    credit_memo_number = table.Column<string>(type: "longtext", nullable: false),
                    credit_memo_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    credit_memo_due_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    credit_memo_total = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    memo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ar_invoice_id = table.Column<int>(type: "int", nullable: true),
                    order_header_id = table.Column<int>(type: "int", nullable: true),
                    credit_reason = table.Column<string>(type: "longtext", nullable: false),
                    is_approved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_applied = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditMemoHeaders", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CreditMemoLines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    credit_memo_header_id = table.Column<int>(type: "int", nullable: false),
                    line_number = table.Column<int>(type: "int", nullable: false),
                    line_total = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    qty_credited = table.Column<int>(type: "int", nullable: false),
                    gl_account = table.Column<string>(type: "longtext", nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: true),
                    ar_invoice_line_id = table.Column<int>(type: "int", nullable: true),
                    order_line_id = table.Column<int>(type: "int", nullable: true),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditMemoLines", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    customer_number = table.Column<int>(type: "int", nullable: false, defaultValue: 10000),
                    customer_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    customer_description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    phone = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false),
                    fax = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: true),
                    general_email = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: true),
                    website = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true),
                    category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    payment_terms = table.Column<string>(type: "longtext", nullable: false),
                    is_taxable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    tax_rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "document_uploads",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    rev_num = table.Column<int>(type: "int", nullable: false),
                    document_object_id = table.Column<int>(type: "int", nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_uploads", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "document_uploads_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    parent_category_id = table.Column<int>(type: "int", nullable: true),
                    category_name = table.Column<string>(type: "longtext", nullable: false),
                    internal_category_name = table.Column<string>(type: "longtext", nullable: false),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_uploads_categories", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "document_uploads_object",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    friendly_name = table.Column<string>(type: "longtext", nullable: false),
                    internal_name = table.Column<string>(type: "varchar(255)", nullable: false),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
                    requires_approval = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    approve_by_id = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_uploads_object", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "document_uploads_object_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    document_upload_category_id = table.Column<int>(type: "int", nullable: false),
                    document_upload_object_id = table.Column<int>(type: "int", nullable: false),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_uploads_object_categories", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inventory_counts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    product_name = table.Column<string>(type: "longtext", nullable: false),
                    current_stock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    required_stock = table.Column<int>(type: "int", nullable: false),
                    reorder_level = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    on_hand = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    reserved = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    on_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    to_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_units_sold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_units_received = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_units_shipped = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_on_purchased = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_counts", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "key_value_stores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    key = table.Column<string>(type: "varchar(255)", nullable: false),
                    value = table.Column<string>(type: "longtext", nullable: false),
                    int_value = table.Column<int>(type: "int", nullable: true),
                    module_id = table.Column<string>(type: "longtext", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_key_value_stores", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "leads",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    cell_phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    company_name = table.Column<string>(type: "longtext", nullable: false),
                    lead_stage = table.Column<string>(type: "varchar(255)", nullable: false),
                    time_zone = table.Column<string>(type: "longtext", nullable: true),
                    address_line1 = table.Column<string>(type: "longtext", nullable: true),
                    address_line2 = table.Column<string>(type: "longtext", nullable: true),
                    city = table.Column<string>(type: "longtext", nullable: true),
                    state = table.Column<string>(type: "longtext", nullable: true),
                    zip = table.Column<string>(type: "longtext", nullable: true),
                    country = table.Column<string>(type: "longtext", nullable: true),
                    is_converted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    converted_customer_id = table.Column<int>(type: "int", nullable: true),
                    converted_contact_id = table.Column<int>(type: "int", nullable: true),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leads", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "logs_error",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    error_severity = table.Column<int>(type: "int", nullable: false),
                    source = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false),
                    method = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    error_message = table.Column<string>(type: "varchar(5000)", maxLength: 5000, nullable: false),
                    inner_message = table.Column<string>(type: "varchar(5000)", maxLength: 5000, nullable: true),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_error", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "logs_general",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    category = table.Column<string>(type: "longtext", nullable: false),
                    message = table.Column<string>(type: "longtext", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_general", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "module_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    module_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    module_name = table.Column<string>(type: "longtext", nullable: false),
                    permission_name = table.Column<string>(type: "longtext", nullable: false),
                    internal_permission_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    read = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    write = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    edit = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    delete = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    requires_admin = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    requires_management = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    requires_guest = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_permissions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    object_name = table.Column<string>(type: "longtext", nullable: false),
                    object_id = table.Column<int>(type: "int", nullable: false),
                    alert_text = table.Column<string>(type: "longtext", nullable: false),
                    notified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    notification_read = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "states",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    country_id = table.Column<int>(type: "int", nullable: false),
                    state_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    iso2 = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_states", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    transaction_type = table.Column<int>(type: "int", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    object_reference_id = table.Column<int>(type: "int", nullable: false),
                    object_sub_reference_id = table.Column<int>(type: "int", nullable: true),
                    units_sold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    units_shipped = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    units_purchased = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    units_received = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    purchased_unit_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    sold_unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    first_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    password_salt = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    employee_number = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    department = table.Column<string>(type: "longtext", nullable: true),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_external_user = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_admin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_management = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_guest = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vendors",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    vendor_number = table.Column<int>(type: "int", nullable: false, defaultValue: 10000),
                    vendor_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    vendor_description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    address_id = table.Column<int>(type: "int", nullable: false),
                    phone = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false),
                    fax = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: true),
                    general_email = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: true),
                    website = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true),
                    category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_critial_vendor = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    approved_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    audit_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    audit_by = table.Column<int>(type: "int", nullable: true),
                    retired_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    retired_by = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendors", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_headers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    order_number = table.Column<int>(type: "int", nullable: false, defaultValue: 10000),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    ship_to_address_id = table.Column<int>(type: "int", nullable: false),
                    shipping_method = table.Column<string>(type: "longtext", nullable: false),
                    billing_address_id = table.Column<int>(type: "int", nullable: false),
                    pay_method = table.Column<string>(type: "longtext", nullable: false),
                    opportunity_id = table.Column<int>(type: "int", nullable: true),
                    order_type = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    revision_number = table.Column<int>(type: "int", nullable: false),
                    order_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    required_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    po_number = table.Column<string>(type: "longtext", nullable: true),
                    price = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    tax = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    shipping_cost = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    deleted_reason = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    canceled_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_canceled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    canceled_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    canceled_by = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_headers", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_headers_addresses_ship_to_address_id",
                        column: x => x.ship_to_address_id,
                        principalTable: "addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shipment_headers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    order_header_id = table.Column<int>(type: "int", nullable: false),
                    shipment_number = table.Column<int>(type: "int", nullable: false, defaultValue: 10000),
                    address_id = table.Column<int>(type: "int", nullable: false),
                    revision_number = table.Column<int>(type: "int", nullable: false),
                    units_to_ship = table.Column<int>(type: "int", nullable: false),
                    units_shipped = table.Column<int>(type: "int", nullable: false),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_released = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_canceled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ship_via = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    ship_attn = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    freight_carrier = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    freight_charge_amount = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    tax = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    completed_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    completed_by = table.Column<int>(type: "int", nullable: true),
                    canceled_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    canceled_by = table.Column<int>(type: "int", nullable: true),
                    canceled_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_headers", x => x.id);
                    table.ForeignKey(
                        name: "FK_shipment_headers_addresses_address_id",
                        column: x => x.address_id,
                        principalTable: "addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ap_invoice_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ap_invoice_header_id = table.Column<int>(type: "int", nullable: false),
                    line_number = table.Column<int>(type: "int", nullable: false),
                    line_total = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    qty_invoiced = table.Column<int>(type: "int", nullable: false),
                    gl_account = table.Column<string>(type: "varchar(255)", nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    association_object_id = table.Column<int>(type: "int", nullable: true),
                    association_object_line_id = table.Column<int>(type: "int", nullable: true),
                    association_is_purchase_order = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    association_is_sales_order = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    association_is_ar_invoice = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ap_invoice_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_ap_invoice_lines_ap_invoice_headers_ap_invoice_header_id",
                        column: x => x.ap_invoice_header_id,
                        principalTable: "ap_invoice_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer_addresses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    address_id = table.Column<int>(type: "int", nullable: false),
                    address_type_id = table.Column<int>(type: "int", nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_addresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_addresses_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "opportunities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    opportunity_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    contact_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: false),
                    stage = table.Column<string>(type: "varchar(255)", nullable: false),
                    win_chance = table.Column<int>(type: "int", precision: 3, nullable: false),
                    expected_close = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opportunities", x => x.id);
                    table.ForeignKey(
                        name: "FK_opportunities_contacts_contact_id",
                        column: x => x.contact_id,
                        principalTable: "contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_opportunities_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "document_uploads_revisions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    document_upload_id = table.Column<int>(type: "int", nullable: false),
                    document_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    document_path = table.Column<string>(type: "varchar(1500)", maxLength: 1500, nullable: false),
                    document_type = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    rev_num = table.Column<int>(type: "int", nullable: false),
                    rejected_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    approved_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    rejected_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    rejected_by = table.Column<int>(type: "int", nullable: true),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_uploads_revisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_uploads_revisions_document_uploads_document_upload_~",
                        column: x => x.document_upload_id,
                        principalTable: "document_uploads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "document_uploads_object_tag_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    document_object_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", nullable: false),
                    is_required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_uploads_object_tag_template", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_uploads_object_tag_template_document_uploads_object~",
                        column: x => x.document_object_id,
                        principalTable: "document_uploads_object",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    module_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    read = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    write = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    edit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    delete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    requires_admin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    requires_management = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    requires_guest = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    session_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    session_expires = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    vendor_id = table.Column<int>(type: "int", nullable: false),
                    product_class = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    identifier1 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    identifier2 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    identifier3 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    product_name = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    internal_description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    external_description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    required_stock_level = table.Column<int>(type: "int", nullable: false),
                    required_reorder_level = table.Column<int>(type: "int", nullable: false),
                    required_min_order = table.Column<int>(type: "int", nullable: false),
                    our_cost = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    sales_price = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    list_price = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    guid = table.Column<string>(type: "varchar(255)", nullable: false),
                    rfid_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    is_taxable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_stock = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_material = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_rental_item = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_sales_item = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_labor = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_shippable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_retired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    retired_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    retired_by = table.Column<int>(type: "int", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "purchase_order_headers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    vendor_id = table.Column<int>(type: "int", nullable: false),
                    po_type = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    revision_number = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    po_number = table.Column<int>(type: "int", nullable: false, defaultValue: 10000),
                    price = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    tax = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    deleted_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    canceled_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_canceled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    completed_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    completed_by = table.Column<int>(type: "int", nullable: true),
                    canceled_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    canceled_by = table.Column<int>(type: "int", nullable: true),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_headers", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_headers_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    order_header_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    line_number = table.Column<int>(type: "int", nullable: false),
                    opportunity_line_id = table.Column<int>(type: "int", nullable: true),
                    line_description = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_lines_order_headers_order_header_id",
                        column: x => x.order_header_id,
                        principalTable: "order_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "production_order_headers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    order_header_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(255)", nullable: true),
                    priority_id = table.Column<int>(type: "int", nullable: false),
                    planned_start_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    planned_complete_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actual_completed_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: true),
                    production_lead_minutes = table.Column<int>(type: "int", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_order_headers", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_order_headers_order_headers_order_header_id",
                        column: x => x.order_header_id,
                        principalTable: "order_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "opportunity_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    opportunity_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    line_number = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opportunity_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_opportunity_lines_opportunities_opportunity_id",
                        column: x => x.opportunity_id,
                        principalTable: "opportunities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "document_uploads_revisions_tag",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    document_upload_revision_id = table.Column<int>(type: "int", nullable: false),
                    document_upload_object_tag_id = table.Column<int>(type: "int", nullable: false),
                    tag_name = table.Column<string>(type: "varchar(255)", nullable: false),
                    tag_value = table.Column<string>(type: "varchar(255)", nullable: false),
                    is_required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_uploads_revisions_tag", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_uploads_revisions_tag_document_uploads_revisions_do~",
                        column: x => x.document_upload_revision_id,
                        principalTable: "document_uploads_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "boms",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    parent_product_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    parent_bom_id = table.Column<int>(type: "int", nullable: true),
                    order_number = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    instructions = table.Column<string>(type: "longtext", nullable: true),
                    guid = table.Column<string>(type: "varchar(255)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boms", x => x.id);
                    table.ForeignKey(
                        name: "FK_boms_products_parent_product_id",
                        column: x => x.parent_product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_boms_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_attributes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    attribute_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    attribute_value = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    attribute_value2 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    attribute_value3 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_attributes", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_attributes_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "purchase_order_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    purchase_order_header_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    revision_number = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    line_number = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    tax = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    is_taxable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_canceled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_lines_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchase_order_lines_purchase_order_headers_purchase_order_h~",
                        column: x => x.purchase_order_header_id,
                        principalTable: "purchase_order_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "purchase_order_receive_headers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    purchase_order_id = table.Column<int>(type: "int", nullable: false),
                    units_ordered = table.Column<int>(type: "int", nullable: false),
                    units_received = table.Column<int>(type: "int", nullable: false),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_canceled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    canceled_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    completed_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    canceled_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    canceled_by = table.Column<int>(type: "int", nullable: true),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_receive_headers", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_receive_headers_purchase_order_headers_purcha~",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_order_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ar_invoice_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ar_invoice_header_id = table.Column<int>(type: "int", nullable: false),
                    line_number = table.Column<int>(type: "int", nullable: false),
                    order_line_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    line_description = table.Column<string>(type: "longtext", nullable: false),
                    order_qty = table.Column<int>(type: "int", nullable: false),
                    invoice_qty = table.Column<int>(type: "int", nullable: false),
                    line_total = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    line_tax = table.Column<decimal>(type: "decimal(14,3)", precision: 14, scale: 3, nullable: false),
                    is_taxable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ar_invoice_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_ar_invoice_lines_ar_invoice_headers_ar_invoice_header_id",
                        column: x => x.ar_invoice_header_id,
                        principalTable: "ar_invoice_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ar_invoice_lines_order_lines_order_line_id",
                        column: x => x.order_line_id,
                        principalTable: "order_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ar_invoice_lines_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_line_attributes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    order_line_id = table.Column<int>(type: "int", nullable: false),
                    attribute_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    attribute_value = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    attribute_value2 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    attribute_value3 = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_line_attributes", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_line_attributes_order_lines_order_line_id",
                        column: x => x.order_line_id,
                        principalTable: "order_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shipment_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    shipment_header_id = table.Column<int>(type: "int", nullable: false),
                    order_line_id = table.Column<int>(type: "int", nullable: false),
                    revision_number = table.Column<int>(type: "int", nullable: false),
                    units_to_ship = table.Column<int>(type: "int", nullable: false),
                    units_shipped = table.Column<int>(type: "int", nullable: false),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_canceled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    completed_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    completed_by = table.Column<int>(type: "int", nullable: true),
                    canceled_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    canceled_by = table.Column<int>(type: "int", nullable: true),
                    canceled_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_shipment_lines_order_lines_order_line_id",
                        column: x => x.order_line_id,
                        principalTable: "order_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shipment_lines_shipment_headers_shipment_header_id",
                        column: x => x.shipment_header_id,
                        principalTable: "shipment_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "production_order_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    production_order_header_id = table.Column<int>(type: "int", nullable: false),
                    order_line_id = table.Column<int>(type: "int", nullable: false),
                    line_number = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    production_lead_minutes = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(255)", nullable: false),
                    started_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    completed_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guid = table.Column<string>(type: "longtext", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_order_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_order_lines_order_lines_order_line_id",
                        column: x => x.order_line_id,
                        principalTable: "order_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_production_order_lines_production_order_headers_production_o~",
                        column: x => x.production_order_header_id,
                        principalTable: "production_order_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "purchase_order_receive_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    purchase_order_receive_header_id = table.Column<int>(type: "int", nullable: false),
                    purchase_order_line_id = table.Column<int>(type: "int", nullable: false),
                    units_ordered = table.Column<int>(type: "int", nullable: false),
                    units_received = table.Column<int>(type: "int", nullable: false),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_canceled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    canceled_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    completed_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    canceled_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    canceled_by = table.Column<int>(type: "int", nullable: true),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_receive_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_receive_lines_purchase_order_lines_purchase_o~",
                        column: x => x.purchase_order_line_id,
                        principalTable: "purchase_order_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchase_order_receive_lines_purchase_order_receive_headers_~",
                        column: x => x.purchase_order_receive_header_id,
                        principalTable: "purchase_order_receive_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "purchase_order_receive_uploads",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    purchase_order_receive_header_id = table.Column<int>(type: "int", nullable: false),
                    document_upload_id = table.Column<int>(type: "int", nullable: false),
                    guid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    created_on_timezone = table.Column<string>(type: "longtext", nullable: false),
                    created_on_string = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_on = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_by = table.Column<int>(type: "int", nullable: false),
                    updated_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    updated_on_string = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_on_timezone = table.Column<string>(type: "longtext", nullable: true),
                    deleted_on_string = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_receive_uploads", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_receive_uploads_document_uploads_document_upl~",
                        column: x => x.document_upload_id,
                        principalTable: "document_uploads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchase_order_receive_uploads_purchase_order_receive_header~",
                        column: x => x.purchase_order_receive_header_id,
                        principalTable: "purchase_order_receive_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "document_uploads_categories",
                columns: new[] { "id", "category_name", "created_by", "created_on", "created_on_string", "created_on_timezone", "deleted_by", "deleted_on", "deleted_on_string", "deleted_on_timezone", "guid", "internal_category_name", "is_deleted", "parent_category_id", "updated_by", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[,]
                {
                    { 1, "Accounting", 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "dc6bd3b8-962b-4d12-8c84-588fd8928695", "accounting", false, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00" },
                    { 2, "Sales", 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "5d3fb88f-43fe-41c4-8807-be244cbebda7", "sales", false, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00" },
                    { 3, "Customers", 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "57225634-3e9f-46c7-bd27-48cbf4511d26", "customer", false, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00" },
                    { 4, "Service", 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "c668a5a9-4c57-4219-be3d-93e8c995a4c9", "service", false, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00" },
                    { 5, "Manufacturing", 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "0a636bf2-51a6-407d-b885-a366f0b2013c", "manufacturing", false, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00" },
                    { 6, "Engineering", 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "c3286967-5285-4ea1-a78e-46f18c9ec2b9", "engineering", false, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(7271), "2025-10-05 17:06:27Z", "-05:00" }
                });

            migrationBuilder.InsertData(
                table: "document_uploads_object",
                columns: new[] { "id", "approve_by_id", "created_by", "created_on", "created_on_string", "created_on_timezone", "deleted_by", "deleted_on", "deleted_on_string", "deleted_on_timezone", "friendly_name", "guid", "internal_name", "is_deleted", "requires_approval", "updated_by", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[,]
                {
                    { 1, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "AR Invoice", "6a98c7ed-1478-4684-9070-9f60f42b9c2c", "ar_invoice", false, false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00" },
                    { 2, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "AP Invoice", "de6bd73f-88bd-4862-8a7b-0a618396641f", "ap_invoice", false, false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00" },
                    { 3, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "Tax Exempt Form", "b86bfbe7-d7ed-4be0-9ff2-06a370e48553", "tax_exempt_form", false, false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00" },
                    { 4, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "Customer EIN/TIN Form", "49f4800d-673b-4ae8-aff3-2b700fb1df3b", "customer_formation_form", false, false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00" },
                    { 5, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "PO Receive Upload", "6af485a4-402e-4476-82c2-e91a4d3f83fa", "po_receive_upload", false, false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00" },
                    { 6, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "CAD Drawings", "da0af931-7424-460f-956d-e43a69b00f81", "cad_drawings", false, false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00" },
                    { 7, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "Service Contracts", "857d4113-ab2d-489b-a267-ada6ca7d411d", "service_constracts", false, false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00" },
                    { 8, null, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, "Internal Price Lists", "212cc82c-b151-45a3-997d-bd06d05fa327", "internal_price_listes", false, false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 210, DateTimeKind.Utc).AddTicks(6417), "2025-10-05 17:06:27Z", "-05:00" }
                });

            migrationBuilder.InsertData(
                table: "document_uploads_object_categories",
                columns: new[] { "id", "created_by", "created_on", "created_on_string", "created_on_timezone", "deleted_by", "deleted_on", "deleted_on_string", "deleted_on_timezone", "document_upload_category_id", "document_upload_object_id", "guid", "is_deleted", "updated_by", "updated_on", "updated_on_string", "updated_on_timezone" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, 1, 1, "bff44e3f-d330-49ea-ae04-a310b30e362c", false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00" },
                    { 2, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, 1, 2, "96bf8fab-3095-4617-87f0-15db6a97f817", false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00" },
                    { 3, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, 3, 3, "dd555129-83da-4b71-a50f-9102792487d1", false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00" },
                    { 4, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, 3, 4, "cdd23bbe-5e33-41c8-9eab-c8c1f383ec02", false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00" },
                    { 5, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, 5, 5, "03230d3a-f849-459a-b444-bcaa4e3abb18", false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00" },
                    { 6, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, 6, 6, "8a20d9ee-5cf2-4402-8310-c4e607457377", false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00" },
                    { 7, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, 4, 7, "906df905-1bd1-4a9f-9ba4-427037998aec", false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00" },
                    { 8, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00", null, null, null, null, 2, 8, "264ab60a-6aef-41fd-adcb-fba3265cb572", false, 1, new DateTime(2025, 10, 5, 17, 6, 27, 221, DateTimeKind.Utc).AddTicks(5645), "2025-10-05 17:06:27Z", "-05:00" }
                });

            migrationBuilder.InsertData(
                table: "document_uploads_object_tag_template",
                columns: new[] { "id", "document_object_id", "is_deleted", "is_required", "name" },
                values: new object[,]
                {
                    { 1, 1, false, true, "AR Invoice Number" },
                    { 2, 1, false, true, "Customer Number" },
                    { 3, 1, false, true, "Customer Name" },
                    { 4, 2, false, true, "AP Invoice Number" },
                    { 5, 2, false, true, "Vendor Number" },
                    { 6, 2, false, true, "Vendor Name" },
                    { 7, 3, false, true, "Customer Number" },
                    { 8, 3, false, true, "Customer Name" },
                    { 9, 4, false, true, "Customer Number" },
                    { 10, 4, false, true, "Customer Name" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_activities_activity_type",
                table: "activities",
                column: "activity_type");

            migrationBuilder.CreateIndex(
                name: "IX_activities_customer_id",
                table: "activities",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_activities_guid",
                table: "activities",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_activities_opportunity_id",
                table: "activities",
                column: "opportunity_id");

            migrationBuilder.CreateIndex(
                name: "IX_activities_owner_id",
                table: "activities",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_activities_start_date",
                table: "activities",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "IX_activities_status",
                table: "activities",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_addresses_guid",
                table: "addresses",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_headers_invoice_number",
                table: "ap_invoice_headers",
                column: "invoice_number");

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_headers_vendor_id",
                table: "ap_invoice_headers",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_headers_vendor_id_is_deleted",
                table: "ap_invoice_headers",
                columns: new[] { "vendor_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_headers_vendor_id_is_paid",
                table: "ap_invoice_headers",
                columns: new[] { "vendor_id", "is_paid" });

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_lines_ap_invoice_header_id",
                table: "ap_invoice_lines",
                column: "ap_invoice_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_lines_association_object_id_association_object_li~",
                table: "ap_invoice_lines",
                columns: new[] { "association_object_id", "association_object_line_id" });

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_lines_gl_account",
                table: "ap_invoice_lines",
                column: "gl_account");

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_headers_customer_id",
                table: "ar_invoice_headers",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_headers_order_header_id",
                table: "ar_invoice_headers",
                column: "order_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_lines_ar_invoice_header_id",
                table: "ar_invoice_lines",
                column: "ar_invoice_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_lines_ar_invoice_header_id_is_deleted",
                table: "ar_invoice_lines",
                columns: new[] { "ar_invoice_header_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_lines_order_line_id",
                table: "ar_invoice_lines",
                column: "order_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_lines_product_id",
                table: "ar_invoice_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_boms_guid",
                table: "boms",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_boms_parent_product_id",
                table: "boms",
                column: "parent_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_boms_product_id",
                table: "boms",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_object_guid",
                table: "comments",
                column: "object_guid");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_customer_id",
                table: "contacts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_countries_iso3",
                table: "countries",
                column: "iso3");

            migrationBuilder.CreateIndex(
                name: "IX_customer_addresses_customer_id",
                table: "customer_addresses",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_addresses_guid",
                table: "customer_addresses",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_customers_customer_number",
                table: "customers",
                column: "customer_number");

            migrationBuilder.CreateIndex(
                name: "IX_customers_guid",
                table: "customers",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_document_object_id",
                table: "document_uploads",
                column: "document_object_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_categories_guid",
                table: "document_uploads_categories",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_object_guid",
                table: "document_uploads_object",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_object_internal_name",
                table: "document_uploads_object",
                column: "internal_name");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_object_categories_guid",
                table: "document_uploads_object_categories",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_object_tag_template_document_object_id",
                table: "document_uploads_object_tag_template",
                column: "document_object_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_object_tag_template_name",
                table: "document_uploads_object_tag_template",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_revisions_document_name",
                table: "document_uploads_revisions",
                column: "document_name");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_revisions_document_upload_id",
                table: "document_uploads_revisions",
                column: "document_upload_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_revisions_guid",
                table: "document_uploads_revisions",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_revisions_tag_document_upload_object_tag_id",
                table: "document_uploads_revisions_tag",
                column: "document_upload_object_tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_revisions_tag_document_upload_revision_id",
                table: "document_uploads_revisions_tag",
                column: "document_upload_revision_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_revisions_tag_guid",
                table: "document_uploads_revisions_tag",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_document_uploads_revisions_tag_tag_name_tag_value",
                table: "document_uploads_revisions_tag",
                columns: new[] { "tag_name", "tag_value" });

            migrationBuilder.CreateIndex(
                name: "IX_inventory_counts_id",
                table: "inventory_counts",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_key_value_stores_key",
                table: "key_value_stores",
                column: "key");

            migrationBuilder.CreateIndex(
                name: "IX_leads_lead_stage",
                table: "leads",
                column: "lead_stage");

            migrationBuilder.CreateIndex(
                name: "IX_leads_owner_id",
                table: "leads",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_leads_owner_id_lead_stage_is_deleted",
                table: "leads",
                columns: new[] { "owner_id", "lead_stage", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_module_permissions_internal_permission_name",
                table: "module_permissions",
                column: "internal_permission_name");

            migrationBuilder.CreateIndex(
                name: "IX_module_permissions_module_id",
                table: "module_permissions",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_module_permissions_module_id_internal_permission_name",
                table: "module_permissions",
                columns: new[] { "module_id", "internal_permission_name" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_object_id",
                table: "notifications",
                column: "object_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_contact_id",
                table: "opportunities",
                column: "contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_customer_id",
                table: "opportunities",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_customer_id_stage",
                table: "opportunities",
                columns: new[] { "customer_id", "stage" });

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_owner_id",
                table: "opportunities",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_owner_id_is_deleted",
                table: "opportunities",
                columns: new[] { "owner_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_stage",
                table: "opportunities",
                column: "stage");

            migrationBuilder.CreateIndex(
                name: "IX_opportunity_lines_guid",
                table: "opportunity_lines",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_opportunity_lines_opportunity_id",
                table: "opportunity_lines",
                column: "opportunity_id");

            migrationBuilder.CreateIndex(
                name: "IX_opportunity_lines_product_id",
                table: "opportunity_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_headers_guid",
                table: "order_headers",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_order_headers_order_number",
                table: "order_headers",
                column: "order_number");

            migrationBuilder.CreateIndex(
                name: "IX_order_headers_order_number_order_type",
                table: "order_headers",
                columns: new[] { "order_number", "order_type" });

            migrationBuilder.CreateIndex(
                name: "IX_order_headers_order_number_order_type_revision_number",
                table: "order_headers",
                columns: new[] { "order_number", "order_type", "revision_number" });

            migrationBuilder.CreateIndex(
                name: "IX_order_headers_order_number_revision_number",
                table: "order_headers",
                columns: new[] { "order_number", "revision_number" });

            migrationBuilder.CreateIndex(
                name: "IX_order_headers_ship_to_address_id",
                table: "order_headers",
                column: "ship_to_address_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_line_attributes_order_line_id",
                table: "order_line_attributes",
                column: "order_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_lines_guid",
                table: "order_lines",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_order_lines_order_header_id",
                table: "order_lines",
                column: "order_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_lines_product_id",
                table: "order_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_attributes_attribute_name",
                table: "product_attributes",
                column: "attribute_name");

            migrationBuilder.CreateIndex(
                name: "IX_product_attributes_product_id",
                table: "product_attributes",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_order_headers_order_header_id",
                table: "production_order_headers",
                column: "order_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_order_headers_status",
                table: "production_order_headers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_production_order_lines_order_line_id",
                table: "production_order_lines",
                column: "order_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_order_lines_production_order_header_id",
                table: "production_order_lines",
                column: "production_order_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_order_lines_status",
                table: "production_order_lines",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_products_guid",
                table: "products",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_products_identifier1",
                table: "products",
                column: "identifier1");

            migrationBuilder.CreateIndex(
                name: "IX_products_identifier2",
                table: "products",
                column: "identifier2");

            migrationBuilder.CreateIndex(
                name: "IX_products_identifier3",
                table: "products",
                column: "identifier3");

            migrationBuilder.CreateIndex(
                name: "IX_products_vendor_id",
                table: "products",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_headers_guid",
                table: "purchase_order_headers",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_headers_po_number",
                table: "purchase_order_headers",
                column: "po_number");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_headers_po_number_po_type",
                table: "purchase_order_headers",
                columns: new[] { "po_number", "po_type" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_headers_po_number_po_type_revision_number",
                table: "purchase_order_headers",
                columns: new[] { "po_number", "po_type", "revision_number" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_headers_po_number_revision_number",
                table: "purchase_order_headers",
                columns: new[] { "po_number", "revision_number" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_headers_vendor_id",
                table: "purchase_order_headers",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_guid",
                table: "purchase_order_lines",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_product_id",
                table: "purchase_order_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_purchase_order_header_id",
                table: "purchase_order_lines",
                column: "purchase_order_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_purchase_order_header_id_is_canceled",
                table: "purchase_order_lines",
                columns: new[] { "purchase_order_header_id", "is_canceled" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_purchase_order_header_id_is_complete",
                table: "purchase_order_lines",
                columns: new[] { "purchase_order_header_id", "is_complete" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_purchase_order_header_id_is_deleted",
                table: "purchase_order_lines",
                columns: new[] { "purchase_order_header_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_purchase_order_header_id_line_number",
                table: "purchase_order_lines",
                columns: new[] { "purchase_order_header_id", "line_number" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_purchase_order_header_id_revision_numbe~",
                table: "purchase_order_lines",
                columns: new[] { "purchase_order_header_id", "revision_number", "line_number" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_purchase_order_header_id_revision_number",
                table: "purchase_order_lines",
                columns: new[] { "purchase_order_header_id", "revision_number" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_headers_guid",
                table: "purchase_order_receive_headers",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_headers_purchase_order_id",
                table: "purchase_order_receive_headers",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_headers_purchase_order_id_is_canceled",
                table: "purchase_order_receive_headers",
                columns: new[] { "purchase_order_id", "is_canceled" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_headers_purchase_order_id_is_complete",
                table: "purchase_order_receive_headers",
                columns: new[] { "purchase_order_id", "is_complete" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_headers_purchase_order_id_is_deleted",
                table: "purchase_order_receive_headers",
                columns: new[] { "purchase_order_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_lines_guid",
                table: "purchase_order_receive_lines",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_lines_purchase_order_line_id",
                table: "purchase_order_receive_lines",
                column: "purchase_order_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_lines_purchase_order_receive_header_~1",
                table: "purchase_order_receive_lines",
                columns: new[] { "purchase_order_receive_header_id", "is_complete" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_lines_purchase_order_receive_header_i~",
                table: "purchase_order_receive_lines",
                columns: new[] { "purchase_order_receive_header_id", "is_canceled" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_lines_purchase_order_receive_header_id",
                table: "purchase_order_receive_lines",
                column: "purchase_order_receive_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_uploads_document_upload_id",
                table: "purchase_order_receive_uploads",
                column: "document_upload_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_receive_uploads_purchase_order_receive_header~",
                table: "purchase_order_receive_uploads",
                column: "purchase_order_receive_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_role_id",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_headers_address_id",
                table: "shipment_headers",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_headers_guid",
                table: "shipment_headers",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_headers_order_header_id",
                table: "shipment_headers",
                column: "order_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_headers_order_header_id_is_canceled",
                table: "shipment_headers",
                columns: new[] { "order_header_id", "is_canceled" });

            migrationBuilder.CreateIndex(
                name: "IX_shipment_headers_order_header_id_is_complete",
                table: "shipment_headers",
                columns: new[] { "order_header_id", "is_complete" });

            migrationBuilder.CreateIndex(
                name: "IX_shipment_headers_order_header_id_is_deleted",
                table: "shipment_headers",
                columns: new[] { "order_header_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_shipment_headers_shipment_number",
                table: "shipment_headers",
                column: "shipment_number");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lines_guid",
                table: "shipment_lines",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lines_order_line_id",
                table: "shipment_lines",
                column: "order_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lines_order_line_id_is_canceled",
                table: "shipment_lines",
                columns: new[] { "order_line_id", "is_canceled" });

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lines_order_line_id_is_complete",
                table: "shipment_lines",
                columns: new[] { "order_line_id", "is_complete" });

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lines_order_line_id_is_deleted",
                table: "shipment_lines",
                columns: new[] { "order_line_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lines_shipment_header_id",
                table: "shipment_lines",
                column: "shipment_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_states_country_id",
                table: "states",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_guid",
                table: "transactions",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_object_reference_id",
                table: "transactions",
                column: "object_reference_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_object_reference_id_transaction_type",
                table: "transactions",
                columns: new[] { "object_reference_id", "transaction_type" });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_object_reference_id_transaction_type_product_id",
                table: "transactions",
                columns: new[] { "object_reference_id", "transaction_type", "product_id" });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_product_id",
                table: "transactions",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_user_id",
                table: "user_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_guid",
                table: "users",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "IX_vendors_guid",
                table: "vendors",
                column: "guid");

            migrationBuilder.CreateIndex(
                name: "IX_vendors_vendor_number",
                table: "vendors",
                column: "vendor_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "ap_invoice_lines");

            migrationBuilder.DropTable(
                name: "ar_invoice_lines");

            migrationBuilder.DropTable(
                name: "boms");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "CreditMemoHeaders");

            migrationBuilder.DropTable(
                name: "CreditMemoLines");

            migrationBuilder.DropTable(
                name: "customer_addresses");

            migrationBuilder.DropTable(
                name: "document_uploads_categories");

            migrationBuilder.DropTable(
                name: "document_uploads_object_categories");

            migrationBuilder.DropTable(
                name: "document_uploads_object_tag_template");

            migrationBuilder.DropTable(
                name: "document_uploads_revisions_tag");

            migrationBuilder.DropTable(
                name: "inventory_counts");

            migrationBuilder.DropTable(
                name: "key_value_stores");

            migrationBuilder.DropTable(
                name: "leads");

            migrationBuilder.DropTable(
                name: "logs_error");

            migrationBuilder.DropTable(
                name: "logs_general");

            migrationBuilder.DropTable(
                name: "module_permissions");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "opportunity_lines");

            migrationBuilder.DropTable(
                name: "order_line_attributes");

            migrationBuilder.DropTable(
                name: "product_attributes");

            migrationBuilder.DropTable(
                name: "production_order_lines");

            migrationBuilder.DropTable(
                name: "purchase_order_receive_lines");

            migrationBuilder.DropTable(
                name: "purchase_order_receive_uploads");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "shipment_lines");

            migrationBuilder.DropTable(
                name: "states");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "ap_invoice_headers");

            migrationBuilder.DropTable(
                name: "ar_invoice_headers");

            migrationBuilder.DropTable(
                name: "document_uploads_object");

            migrationBuilder.DropTable(
                name: "document_uploads_revisions");

            migrationBuilder.DropTable(
                name: "opportunities");

            migrationBuilder.DropTable(
                name: "production_order_headers");

            migrationBuilder.DropTable(
                name: "purchase_order_lines");

            migrationBuilder.DropTable(
                name: "purchase_order_receive_headers");

            migrationBuilder.DropTable(
                name: "order_lines");

            migrationBuilder.DropTable(
                name: "shipment_headers");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "document_uploads");

            migrationBuilder.DropTable(
                name: "contacts");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "purchase_order_headers");

            migrationBuilder.DropTable(
                name: "order_headers");

            migrationBuilder.DropTable(
                name: "vendors");

            migrationBuilder.DropTable(
                name: "addresses");
        }
    }
}
