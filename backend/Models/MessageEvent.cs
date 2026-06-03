namespace backend.Models
{
    public sealed record MessageEvent(
            long Id,
            string SourceDevice,
            string? TargetDevice,
            string Message,
            string? Response,
            bool WasProcessed,
            string? ProcessingError,
            DateTime CreatedAtUtc);
}
