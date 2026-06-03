namespace backend.DTOs
{
    public record DeviceDto(
    int Id,
    string DeviceKey,
    string Name,
    string? Description,
    bool IsActive,
    DateTime? LastSeenAtUtc
);
}
