using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database.Models;

namespace Prometheus.Database.Configurations;

public class DocumentUploadObjectTagTemplateConfiguration : IEntityTypeConfiguration<DocumentUploadObjectTagTemplate>
{
    public void Configure(EntityTypeBuilder<DocumentUploadObjectTagTemplate> builder)
    {
        builder.ToTable("document_uploads_object_tag_template");
        builder.Property(x => x.id).ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.document_object_id });
        builder.HasIndex(x => new { x.name });

        builder.HasData(
            new DocumentUploadObjectTagTemplate()
            {
                id = 1,
                document_object_id = 1,
                name = "AR Invoice Number",
                is_required = true,
            },
            new DocumentUploadObjectTagTemplate()
            {
                id = 2,
                document_object_id = 1,
                name = "Customer Number",
                is_required = true,
            },
            new DocumentUploadObjectTagTemplate()
            {
                id = 3,
                document_object_id = 1,
                name = "Customer Name",
                is_required = true,
            },
            new DocumentUploadObjectTagTemplate()
            {
                id = 4,
                document_object_id = 2,
                name = "AP Invoice Number",
                is_required = true,
            },
            new DocumentUploadObjectTagTemplate()
            {
                id = 5,
                document_object_id = 2,
                name = "Vendor Number",
                is_required = true,
            },
            new DocumentUploadObjectTagTemplate()
            {
                id = 6,
                document_object_id = 2,
                name = "Vendor Name",
                is_required = true,
            },
            new DocumentUploadObjectTagTemplate()
            {
                id = 7,
                document_object_id = 3,
                name = "Customer Number",
                is_required = true,
            },
            new DocumentUploadObjectTagTemplate()
            {
                id = 8,
                document_object_id = 3,
                name = "Customer Name",
                is_required = true,
            },
            new DocumentUploadObjectTagTemplate()
            {
                id = 9,
                document_object_id = 4,
                name = "Customer Number",
                is_required = true,
            },
            new DocumentUploadObjectTagTemplate()
            {
                id = 10,
                document_object_id = 4,
                name = "Customer Name",
                is_required = true,
            }
        );
    }
}
