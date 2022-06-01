using System.Runtime.CompilerServices;
using Box.V2;
using Storage.Common;

namespace Storage.Box.Service;

public class Search: IStorageSearch
{
    private readonly BoxClient _boxClient;

    public Search(BoxClient boxClient)
    {
        _boxClient = boxClient;
    }
    
    public async IAsyncEnumerable<IEnumerable<FileMetaData>> List(string name, [EnumeratorCancellation] CancellationToken ctx)
    {
        var searchResults = await _boxClient.SearchManager.QueryAsync(name);
        
        if (ctx.IsCancellationRequested) yield break;
        
        yield return searchResults.Entries.Select(e => new FileMetaData(e.Name, "Box"));
    }
}