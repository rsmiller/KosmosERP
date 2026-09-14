using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class DocumentUploadObjectCategoryConfiguration : IEntityTypeConfiguration<DocumentUploadObjectCategory>
{
    public void Configure(EntityTypeBuilder<DocumentUploadObjectCategory> builder)
    {
        builder.ToTable("document_uploads_object_categories");
        builder.Property(x => x.id).ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.guid });

        var now = DateTime.UtcNow;

        builder.HasData(
            new DocumentUploadObjectCategory()
            {
                id = 1,
                guid = "bff44e3f-d330-49ea-ae04-a310b30e362c",
                document_upload_category_id = 1,
                document_upload_object_id = 1,
                created_on = now,
                updated_on = now,
                created_on_string = now.ToString("u"),
                created_on_timezone = GetTimezoneAsString(now),
                updated_on_string = now.ToString("u"),
                updated_on_timezone = GetTimezoneAsString(now),
                created_by = "1",
                    updated_by = "1"
            },
            new DocumentUploadObjectCategory()
            {
                id = 2,
                guid = "96bf8fab-3095-4617-87f0-15db6a97f817",
                document_upload_category_id = 1,
                document_upload_object_id = 2,
                created_on = now,
                updated_on = now,
                created_on_string = now.ToString("u"),
                created_on_timezone = GetTimezoneAsString(now),
                updated_on_string = now.ToString("u"),
                updated_on_timezone = GetTimezoneAsString(now),
                created_by = "1",
                updated_by = "1"
            },
            new DocumentUploadObjectCategory()
            {
                id = 3,
                guid = "dd555129-83da-4b71-a50f-9102792487d1",
                document_upload_category_id = 3,
                document_upload_object_id = 3,
                created_on = now,
                updated_on = now,
                created_on_string = now.ToString("u"),
                created_on_timezone = GetTimezoneAsString(now),
                updated_on_string = now.ToString("u"),
                updated_on_timezone = GetTimezoneAsString(now),
                created_by = "1",
                updated_by = "1"
            },
            new DocumentUploadObjectCategory()
            {
                id = 4,
                guid = "cdd23bbe-5e33-41c8-9eab-c8c1f383ec02",
                document_upload_category_id = 3,
                document_upload_object_id = 4,
                created_on = now,
                updated_on = now,
                created_on_string = now.ToString("u"),
                created_on_timezone = GetTimezoneAsString(now),
                updated_on_string = now.ToString("u"),
                updated_on_timezone = GetTimezoneAsString(now),
                created_by = "1",
                updated_by = "1"
            },
            new DocumentUploadObjectCategory()
            {
                id = 5,
                guid = "03230d3a-f849-459a-b444-bcaa4e3abb18",
                document_upload_category_id = 5,
                document_upload_object_id = 5,
                created_on = now,
                updated_on = now,
                created_on_string = now.ToString("u"),
                created_on_timezone = GetTimezoneAsString(now),
                updated_on_string = now.ToString("u"),
                updated_on_timezone = GetTimezoneAsString(now),
                created_by = "1",
                updated_by = "1"
            },
            new DocumentUploadObjectCategory()
            {
                id = 6,
                guid = "8a20d9ee-5cf2-4402-8310-c4e607457377",
                document_upload_category_id = 6,
                document_upload_object_id = 6,
                created_on = now,
                updated_on = now,
                created_on_string = now.ToString("u"),
                created_on_timezone = GetTimezoneAsString(now),
                updated_on_string = now.ToString("u"),
                updated_on_timezone = GetTimezoneAsString(now),
                created_by = "1",
                updated_by = "1"
            },
            new DocumentUploadObjectCategory()
            {
                id = 7,
                guid = "906df905-1bd1-4a9f-9ba4-427037998aec",
                document_upload_category_id = 4,
                document_upload_object_id = 7,
                created_on = now,
                updated_on = now,
                created_on_string = now.ToString("u"),
                created_on_timezone = GetTimezoneAsString(now),
                updated_on_string = now.ToString("u"),
                updated_on_timezone = GetTimezoneAsString(now),
                created_by = "1",
                updated_by = "1"
            },
            new DocumentUploadObjectCategory()
            {
                id = 8,
                guid = "264ab60a-6aef-41fd-adcb-fba3265cb572",
                document_upload_category_id = 2,
                document_upload_object_id = 8,
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
