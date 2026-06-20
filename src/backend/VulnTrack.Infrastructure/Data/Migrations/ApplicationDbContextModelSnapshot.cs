using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VulnTrack.Domain.Enums;
using VulnTrack.Infrastructure.Data;

#nullable disable

namespace VulnTrack.Infrastructure.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("VulnTrack.Domain.Entities.Asset", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("DeletedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("DeletedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Description")
                    .HasMaxLength(2000)
                    .HasColumnType("nvarchar(2000)");

                b.Property<string>("Environment")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<bool>("IsDeleted")
                    .HasColumnType("bit");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>("Owner")
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>("Tags")
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<AssetType>("Type")
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .HasColumnType("nvarchar(30)");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("UpdatedBy")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.HasIndex("Name")
                    .HasDatabaseName("IX_Assets_Name");

                b.HasIndex("Type")
                    .HasDatabaseName("IX_Assets_Type");

                b.ToTable("Assets");

                b.HasQueryFilter("a => !a.IsDeleted");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.Attachment", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("BlobUri")
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasColumnType("nvarchar(1024)");

                b.Property<string>("ContentType")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("FileName")
                    .IsRequired()
                    .HasMaxLength(260)
                    .HasColumnType("nvarchar(260)");

                b.Property<long>("FileSizeBytes")
                    .HasColumnType("bigint");

                b.Property<Guid?>("UploadBatchId")
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("UpdatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<Guid>("VulnerabilityId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.HasIndex("UploadBatchId")
                    .HasDatabaseName("IX_Attachments_UploadBatchId");

                b.HasIndex("VulnerabilityId")
                    .HasDatabaseName("IX_Attachments_VulnerabilityId");

                b.ToTable("Attachments");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.ScheduledReminder", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTimeOffset?>("CancelledAt")
                    .HasColumnType("datetimeoffset");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("FailureReason")
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<string>("Message")
                    .HasMaxLength(2000)
                    .HasColumnType("nvarchar(2000)");

                b.Property<string>("RecipientEmail")
                    .IsRequired()
                    .HasMaxLength(254)
                    .HasColumnType("nvarchar(254)");

                b.Property<string>("RecipientUserId")
                    .HasMaxLength(128)
                    .HasColumnType("nvarchar(128)");

                b.Property<DateTimeOffset>("ScheduledFor")
                    .HasColumnType("datetimeoffset");

                b.Property<DateTimeOffset?>("SentAt")
                    .HasColumnType("datetimeoffset");

                b.Property<ReminderStatus>("Status")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("UpdatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<Guid>("VulnerabilityId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.HasIndex("VulnerabilityId")
                    .HasDatabaseName("IX_ScheduledReminders_VulnerabilityId");

                b.HasIndex("Status", "ScheduledFor")
                    .HasDatabaseName("IX_ScheduledReminders_Status_ScheduledFor");

                b.ToTable("ScheduledReminders");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.StatusUpdate", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Comment")
                    .HasMaxLength(2000)
                    .HasColumnType("nvarchar(2000)");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<VulnerabilityStatus>("NewStatus")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<VulnerabilityStatus>("PreviousStatus")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("UpdatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<Guid>("VulnerabilityId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.HasIndex("CreatedAt")
                    .HasDatabaseName("IX_StatusUpdates_CreatedAt");

                b.HasIndex("VulnerabilityId")
                    .HasDatabaseName("IX_StatusUpdates_VulnerabilityId");

                b.ToTable("StatusUpdates");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.Team", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("DeletedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("DeletedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Description")
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<bool>("IsDeleted")
                    .HasColumnType("bit");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnType("nvarchar(150)");

                b.Property<string>("TeamLeadEmail")
                    .HasMaxLength(254)
                    .HasColumnType("nvarchar(254)");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("UpdatedBy")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.HasIndex("Name")
                    .IsUnique()
                    .HasDatabaseName("IX_Teams_Name");

                b.ToTable("Teams");

                b.HasQueryFilter("t => !t.IsDeleted");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.UploadBatch", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("DeletedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("DeletedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("ErrorSummary")
                    .HasMaxLength(4000)
                    .HasColumnType("nvarchar(4000)");

                b.Property<int>("FailureCount")
                    .HasColumnType("int");

                b.Property<bool>("IsDeleted")
                    .HasColumnType("bit");

                b.Property<string>("OriginalFileName")
                    .IsRequired()
                    .HasMaxLength(260)
                    .HasColumnType("nvarchar(260)");

                b.Property<int>("ProcessedCount")
                    .HasColumnType("int");

                b.Property<string>("RawFileBlobUri")
                    .HasMaxLength(1024)
                    .HasColumnType("nvarchar(1024)");

                b.Property<Guid>("SourceId")
                    .HasColumnType("uniqueidentifier");

                b.Property<UploadBatchStatus>("Status")
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .HasColumnType("nvarchar(30)");

                b.Property<int>("SuccessCount")
                    .HasColumnType("int");

                b.Property<int>("TotalRecords")
                    .HasColumnType("int");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("UpdatedBy")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.HasIndex("CreatedAt")
                    .HasDatabaseName("IX_UploadBatches_CreatedAt");

                b.HasIndex("SourceId")
                    .HasDatabaseName("IX_UploadBatches_SourceId");

                b.HasIndex("Status")
                    .HasDatabaseName("IX_UploadBatches_Status");

                b.ToTable("UploadBatches");

                b.HasQueryFilter("b => !b.IsDeleted");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.Vulnerability", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("AssignedToEmail")
                    .HasMaxLength(254)
                    .HasColumnType("nvarchar(254)");

                b.Property<string>("AssignedToUserId")
                    .HasMaxLength(128)
                    .HasColumnType("nvarchar(128)");

                b.Property<string>("CveId")
                    .HasMaxLength(30)
                    .HasColumnType("nvarchar(30)");

                b.Property<decimal?>("CvssScore")
                    .HasPrecision(4, 1)
                    .HasColumnType("decimal(4,1)");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("DeletedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("DeletedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Description")
                    .IsRequired()
                    .HasMaxLength(8000)
                    .HasColumnType("nvarchar(8000)");

                b.Property<DateTimeOffset?>("DiscoveredAt")
                    .HasColumnType("datetimeoffset");

                b.Property<DateTimeOffset?>("Ecd")
                    .HasColumnType("datetimeoffset");

                b.Property<DateTimeOffset?>("FollowUpDue")
                    .HasColumnType("datetimeoffset");

                b.Property<bool>("IsDeleted")
                    .HasColumnType("bit");

                b.Property<DateTimeOffset>("LastUpdated")
                    .HasColumnType("datetimeoffset");

                b.Property<RemediationPriority>("Priority")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<DateTimeOffset?>("RemediatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("ServerIp")
                    .IsRequired()
                    .HasMaxLength(45)
                    .HasColumnType("nvarchar(45)");

                b.Property<string>("ServerName")
                    .IsRequired()
                    .HasMaxLength(253)
                    .HasColumnType("nvarchar(253)");

                b.Property<Severity>("Severity")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<string>("Solution")
                    .HasMaxLength(8000)
                    .HasColumnType("nvarchar(8000)");

                b.Property<Guid>("SourceId")
                    .HasColumnType("uniqueidentifier");

                b.Property<VulnerabilityStatus>("Status")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<Guid?>("TeamId")
                    .HasColumnType("uniqueidentifier");

                b.Property<Guid?>("UploadBatchId")
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("UpdatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("VulnerabilityNumber")
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnType("nvarchar(30)");

                b.Property<VulnerabilityType>("VulnerabilityType")
                    .HasConversion<string>()
                    .HasMaxLength(40)
                    .HasColumnType("nvarchar(40)");

                b.HasKey("Id");

                b.HasIndex("AssignedToEmail")
                    .HasDatabaseName("IX_Vulnerabilities_AssignedToEmail");

                b.HasIndex("Ecd")
                    .HasDatabaseName("IX_Vulnerabilities_Ecd");

                b.HasIndex("FollowUpDue")
                    .HasDatabaseName("IX_Vulnerabilities_FollowUpDue");

                b.HasIndex("ServerIp")
                    .HasDatabaseName("IX_Vulnerabilities_ServerIp");

                b.HasIndex("ServerName")
                    .HasDatabaseName("IX_Vulnerabilities_ServerName");

                b.HasIndex("Severity")
                    .HasDatabaseName("IX_Vulnerabilities_Severity");

                b.HasIndex("SourceId")
                    .HasDatabaseName("IX_Vulnerabilities_SourceId");

                b.HasIndex("Status")
                    .HasDatabaseName("IX_Vulnerabilities_Status");

                b.HasIndex("TeamId")
                    .HasDatabaseName("IX_Vulnerabilities_TeamId");

                b.HasIndex("VulnerabilityNumber")
                    .IsUnique()
                    .HasDatabaseName("IX_Vulnerabilities_VulnerabilityNumber");

                b.ToTable("Vulnerabilities");

                b.HasQueryFilter("v => !v.IsDeleted");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.VulnerabilityComment", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Body")
                    .IsRequired()
                    .HasMaxLength(4000)
                    .HasColumnType("nvarchar(4000)");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<bool>("IsInternal")
                    .HasColumnType("bit");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("UpdatedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<Guid>("VulnerabilityId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.HasIndex("CreatedAt")
                    .HasDatabaseName("IX_Comments_CreatedAt");

                b.HasIndex("VulnerabilityId")
                    .HasDatabaseName("IX_Comments_VulnerabilityId");

                b.ToTable("Comments");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.VulnerabilitySource", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTimeOffset?>("DeletedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("DeletedBy")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Description")
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit");

                b.Property<bool>("IsDeleted")
                    .HasColumnType("bit");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("datetimeoffset");

                b.Property<string>("UpdatedBy")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.HasIndex("Name")
                    .IsUnique()
                    .HasDatabaseName("IX_VulnerabilitySources_Name");

                b.ToTable("VulnerabilitySources");

                b.HasQueryFilter("s => !s.IsDeleted");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.Attachment", b =>
            {
                b.HasOne("VulnTrack.Domain.Entities.Vulnerability", null)
                    .WithMany("Attachments")
                    .HasForeignKey("VulnerabilityId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.ScheduledReminder", b =>
            {
                b.HasOne("VulnTrack.Domain.Entities.Vulnerability", "Vulnerability")
                    .WithMany("Reminders")
                    .HasForeignKey("VulnerabilityId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Vulnerability");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.StatusUpdate", b =>
            {
                b.HasOne("VulnTrack.Domain.Entities.Vulnerability", null)
                    .WithMany("StatusUpdates")
                    .HasForeignKey("VulnerabilityId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.UploadBatch", b =>
            {
                b.HasOne("VulnTrack.Domain.Entities.VulnerabilitySource", "Source")
                    .WithMany("UploadBatches")
                    .HasForeignKey("SourceId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Source");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.Vulnerability", b =>
            {
                b.HasOne("VulnTrack.Domain.Entities.Team", "Team")
                    .WithMany("Vulnerabilities")
                    .HasForeignKey("TeamId")
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasOne("VulnTrack.Domain.Entities.UploadBatch", "UploadBatch")
                    .WithMany("Vulnerabilities")
                    .HasForeignKey("UploadBatchId")
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasOne("VulnTrack.Domain.Entities.VulnerabilitySource", "Source")
                    .WithMany("Vulnerabilities")
                    .HasForeignKey("SourceId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Source");
                b.Navigation("Team");
                b.Navigation("UploadBatch");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.VulnerabilityComment", b =>
            {
                b.HasOne("VulnTrack.Domain.Entities.Vulnerability", null)
                    .WithMany("Comments")
                    .HasForeignKey("VulnerabilityId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.Team", b =>
            {
                b.Navigation("Vulnerabilities");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.UploadBatch", b =>
            {
                b.Navigation("Vulnerabilities");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.Vulnerability", b =>
            {
                b.Navigation("Attachments");
                b.Navigation("Comments");
                b.Navigation("Reminders");
                b.Navigation("StatusUpdates");
            });

            modelBuilder.Entity("VulnTrack.Domain.Entities.VulnerabilitySource", b =>
            {
                b.Navigation("UploadBatches");
                b.Navigation("Vulnerabilities");
            });
#pragma warning restore 612, 618
        }
    }
}
