using System.Runtime.CompilerServices;
using Dropbox.Api;
using Dropbox.Api.Files;
using Storage.Common;

namespace Storage.Dropbox.Service;

public class Search: IStorageSearch
{
    private readonly DropboxClient _dropboxClient;

    public string StorageProviderName => "Dropbox";

    public Search(DropboxClient dropboxClient)
    {
        _dropboxClient = dropboxClient;
    }

    public async IAsyncEnumerable<IEnumerable<FileMetaData>> List(string name,
        [EnumeratorCancellation] CancellationToken ctx)
    {
        ListFolderResult listResponse;
        do
        {
            if (ctx.IsCancellationRequested) yield break;

            listResponse = await _dropboxClient.Files.ListFolderAsync("", recursive: true);
            yield return listResponse.Entries.Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Select(x => new FileMetaData(x.Name, StorageProviderName) {Id = x.PathLower, IsDirectory = x.IsFolder});
        } while (listResponse.HasMore);
    }
}