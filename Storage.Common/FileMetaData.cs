namespace Storage.Common;

public record FileMetaData(string Name, string StorageProviderName)
{
    public bool IsDirectory { get; init; }

    public string? Id { get; init; }
}