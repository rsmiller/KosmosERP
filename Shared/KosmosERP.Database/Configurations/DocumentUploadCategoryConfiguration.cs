using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Database.Models;

namespace KosmosERP.Database.Configurations;

public class DocumentUploadCategoryConfiguration : IEntityTypeConfiguration<DocumentUploadCategory>
{
    public void Configure(EntityTypeBuilder<DocumentUploadCategory> builder)
    {
        builder.ToTable("document_uploads_categories");
        builder.Property(x => x.id).ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.guid });

        var now = DateTime.UtcNow;

        builder.HasData(
                new DocumentUploadCategory()
                {
                    id = 1,
                    guid = "dc6bd3b8-962b-4d12-8c84-588fd8928695",
                    internal_category_name = "accounting",
                    category_name = "Accounting",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadCategory()
                {
                    id = 2,
                    guid = "5d3fb88f-43fe-41c4-8807-be244cbebda7",
                    internal_category_name = "sales",
                    category_name = "Sales",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadCategory()
                {
                    id = 3,
                    guid = "57225634-3e9f-46c7-bd27-48cbf4511d26",
                    internal_category_name = "customer",
                    category_name = "Customers",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadCategory()
                {
                    id = 4,
                    guid = "c668a5a9-4c57-4219-be3d-93e8c995a4c9",
                    internal_category_name = "service",
                    category_name = "Service",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadCategory()
                {
                    id = 5,
                    guid = "0a636bf2-51a6-407d-b885-a366f0b2013c",
                    internal_category_name = "manufacturing",
                    category_name = "Manufacturing",
                    created_on = now,
                    updated_on = now,
                    created_on_string = now.ToString("u"),
                    created_on_timezone = GetTimezoneAsString(now),
                    updated_on_string = now.ToString("u"),
                    updated_on_timezone = GetTimezoneAsString(now),
                    created_by = "1",
                    updated_by = "1"
                },
                new DocumentUploadCategory()
                {
                    id = 6,
                    guid = "c3286967-5285-4ea1-a78e-46f18c9ec2b9",
                    internal_category_name = "engineering",
                    category_name = "Engineering",
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
