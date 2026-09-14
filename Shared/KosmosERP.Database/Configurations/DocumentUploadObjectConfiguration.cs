using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class DocumentUploadObjectConfiguration : IEntityTypeConfiguration<DocumentUploadObject>
{
    public void Configure(EntityTypeBuilder<DocumentUploadObject> builder)
    {
        builder.ToTable("document_uploads_object");
        builder.Property(x => x.id).ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.internal_name });
        builder.HasIndex(x => new { x.guid });

        builder.HasMany<DocumentUploadObjectTagTemplate>(x => x.object_tags).WithOne().HasForeignKey(x => x.document_object_id).HasPrincipalKey(c => c.id);

        var now = DateTime.UtcNow;

        builder.HasData(
                new DocumentUploadObject()
                {
                    id = 1,
                    guid = "6a98c7ed-1478-4684-9070-9f60f42b9c2c",
                    internal_name = "ar_invoice",
                    friendly_name = "AR Invoice",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                     updated_by = "1"

                },
                new DocumentUploadObject()
                {
                    id = 2,
                    guid = "de6bd73f-88bd-4862-8a7b-0a618396641f",
                    internal_name = "ap_invoice",
                    friendly_name = "AP Invoice",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                updated_by = "1"
                },
                new DocumentUploadObject()
                {
                    id = 3,
                    guid = "b86bfbe7-d7ed-4be0-9ff2-06a370e48553",
                    internal_name = "tax_exempt_form",
                    friendly_name = "Tax Exempt Form",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadObject()
                {
                    id = 4,
                    guid = "49f4800d-673b-4ae8-aff3-2b700fb1df3b",
                    internal_name = "customer_formation_form",
                    friendly_name = "Customer EIN/TIN Form",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadObject()
                {
                    id = 5,
                    guid = "6af485a4-402e-4476-82c2-e91a4d3f83fa",
                    internal_name = "po_receive_upload",
                    friendly_name = "PO Receive Upload",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadObject()
                {
                    id = 6,
                    guid = "da0af931-7424-460f-956d-e43a69b00f81",
                    internal_name = "cad_drawings",
                    friendly_name = "CAD Drawings",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadObject()
                {
                    id = 7,
                    guid = "857d4113-ab2d-489b-a267-ada6ca7d411d",
                    internal_name = "service_constracts",
                    friendly_name = "Service Contracts",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadObject()
                {
                    id = 8,
                    guid = "212cc82c-b151-45a3-997d-bd06d05fa327",
                    internal_name = "internal_price_listes",
                    friendly_name = "Internal Price Lists",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                }
        );
    }
    
    public string GetTimezoneAsString(DateTime the_date)
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(the_date);
        string formattedOffset = offset.ToString(@"hh\:mm");

        if (offset < TimeSpan.Zero)
            formattedOffset = "-" + formattedOffset;
        else
            formattedOffset = "+" + formattedOffset;

        return formattedOffset;
    }
}
