using System.Runtime.CompilerServices;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Storage.Common;

namespace Storage.GoogleDrive.Service;

public class Search: IStorageSearch
{
    private readonly DriveService _service;

    public Search(DriveService service)
    {
        _service = service;
    }

    public string StorageProviderName => "GoogleDrive";

    public async IAsyncEnumerable<IEnumerable<FileMetaData>> List(string name,
        [EnumeratorCancellation] CancellationToken ctx)
    {
        var request = _service.Files.List();
        request.Q = $"name contains '{name}'";
        request.Fields = "nextPageToken, files(id, name)";
        request.PageSize = 100;

        FileList response;
        var pageToken = string.Empty;
        do
        {
            request.PageToken = pageToken;
            response = await request.ExecuteAsync(ctx);
            pageToken = response.NextPageToken;

            yield return response.Files.Select(f => new FileMetaData(f.Name, StorageProviderName)
                {Id = f.Id, IsDirectory = IsFolder(f.MimeType)});
            
        } while (!string.IsNullOrEmpty(response.NextPageToken));
    }


    private bool IsFolder(string mimeType)
    {
        return mimeType == "application/vnd.google-apps.folder";
    }
}