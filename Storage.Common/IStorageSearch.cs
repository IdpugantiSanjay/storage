namespace Storage.Common;

public interface IStorageSearch
{
    IAsyncEnumerable<IEnumerable<FileMetaData>> List(string name, CancellationToken ctx);
}