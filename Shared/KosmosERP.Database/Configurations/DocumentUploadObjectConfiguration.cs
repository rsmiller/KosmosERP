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


        builder.HasData(
                new DocumentUploadObject()
                {
                    id = 1,
                    guid = "6a98c7ed-1478-4684-9070-9f60f42b9c2c",
                    internal_name = "ar_invoice",
                    friendly_name = "AR Invoice"
                },
                new DocumentUploadObject()
                {
                    id = 2,
                    guid = "de6bd73f-88bd-4862-8a7b-0a618396641f",
                    internal_name = "ap_invoice",
                    friendly_name = "AP Invoice"
                },
                new DocumentUploadObject()
                {
                    id = 3,
                    guid = "b86bfbe7-d7ed-4be0-9ff2-06a370e48553",
                    internal_name = "tax_exempt_form",
                    friendly_name = "Tax Exempt Form"
                },
                new DocumentUploadObject()
                {
                    id = 4,
                    guid = "49f4800d-673b-4ae8-aff3-2b700fb1df3b",
                    internal_name = "customer_formation_form",
                    friendly_name = "Customer EIN/TIN Form"
                }
        );
    }
}
