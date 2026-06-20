using FluentAssertions;
using VulnTrack.Domain.Entities;
using VulnTrack.Domain.Enums;
using VulnTrack.Domain.Events;
using Xunit;

namespace VulnTrack.Domain.Tests;

public sealed class UploadBatchTests
{
    private static readonly Guid SourceId = Guid.NewGuid();

    [Fact]
    public void Create_StartsAsQueued()
    {
        var batch = UploadBatch.Create(SourceId, "scan.nessus", "importer");

        batch.Status.Should().Be(UploadBatchStatus.Queued);
        batch.SourceId.Should().Be(SourceId);
        batch.OriginalFileName.Should().Be("scan.nessus");
        batch.CreatedBy.Should().Be("importer");
        batch.ProcessedCount.Should().Be(0);
        batch.SuccessCount.Should().Be(0);
        batch.FailureCount.Should().Be(0);
    }

    [Fact]
    public void Create_WithBlobUri_StoresBlobUri()
    {
        var batch = UploadBatch.Create(SourceId, "scan.nessus", "importer", "https://storage/scan.nessus");
        batch.RawFileBlobUri.Should().Be("https://storage/scan.nessus");
    }

    [Fact]
    public void Start_ChangesStatusToProcessing()
    {
        var batch = UploadBatch.Create(SourceId, "scan.nessus", "importer");
        batch.Start();
        batch.Status.Should().Be(UploadBatchStatus.Processing);
    }

    [Fact]
    public void RecordSuccess_IncrementsProcessedAndSuccessCount()
    {
        var batch = BuildProcessingBatch();
        batch.RecordSuccess();
        batch.RecordSuccess();

        batch.ProcessedCount.Should().Be(2);
        batch.SuccessCount.Should().Be(2);
        batch.FailureCount.Should().Be(0);
    }

    [Fact]
    public void RecordFailure_IncrementsProcessedAndFailureCount()
    {
        var batch = BuildProcessingBatch();
        batch.RecordFailure();

        batch.ProcessedCount.Should().Be(1);
        batch.SuccessCount.Should().Be(0);
        batch.FailureCount.Should().Be(1);
    }

    [Fact]
    public void Complete_NoFailures_StatusIsCompleted()
    {
        var batch = BuildProcessingBatch(successes: 5);
        batch.Complete("importer");
        batch.Status.Should().Be(UploadBatchStatus.Completed);
    }

    [Fact]
    public void Complete_WithFailures_StatusIsCompletedWithErrors()
    {
        var batch = BuildProcessingBatch(successes: 3, failures: 2);
        batch.Complete("importer");
        batch.Status.Should().Be(UploadBatchStatus.CompletedWithErrors);
    }

    [Fact]
    public void Complete_RaisesUploadBatchCompletedEvent()
    {
        var batch = BuildProcessingBatch(successes: 4, failures: 1);
        batch.Complete("importer");

        var evt = batch.DomainEvents.OfType<UploadBatchCompletedEvent>().Single();
        evt.BatchId.Should().Be(batch.Id);
        evt.SourceId.Should().Be(SourceId);
        evt.TotalRecords.Should().Be(5);
        evt.SuccessCount.Should().Be(4);
        evt.FailureCount.Should().Be(1);
    }

    [Fact]
    public void Fail_ChangesStatusToFailed_SetsErrorSummary()
    {
        var batch = UploadBatch.Create(SourceId, "scan.nessus", "importer");

        batch.Fail("Parse error on line 42", "importer");

        batch.Status.Should().Be(UploadBatchStatus.Failed);
        batch.ErrorSummary.Should().Be("Parse error on line 42");
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled()
    {
        var batch = UploadBatch.Create(SourceId, "scan.nessus", "importer");

        batch.Cancel("admin");

        batch.Status.Should().Be(UploadBatchStatus.Cancelled);
        batch.UpdatedBy.Should().Be("admin");
    }

    private static UploadBatch BuildProcessingBatch(int successes = 0, int failures = 0)
    {
        var batch = UploadBatch.Create(SourceId, "scan.nessus", "importer");
        batch.Start();
        batch.SetTotalRecords(successes + failures);
        for (var i = 0; i < successes; i++) batch.RecordSuccess();
        for (var i = 0; i < failures; i++) batch.RecordFailure();
        return batch;
    }
}
