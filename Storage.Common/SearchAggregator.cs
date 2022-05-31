using System.Threading.Channels;

namespace Storage.Common;

public class SearchAggregator: IStorageSearch
{
    private readonly IEnumerable<IStorageSearch> _searchServices;

    public SearchAggregator(IEnumerable<IStorageSearch> searchServices)
    {
        _searchServices = searchServices;
    }


    public IAsyncEnumerable<IEnumerable<FileMetaData>> List(string name, CancellationToken ctx)
    {
        var channel = Channel.CreateUnbounded<IEnumerable<FileMetaData>>();
        foreach (var searchService in _searchServices)
        {
#pragma warning disable CS4014
            EnumerateFiles(searchService, name, channel.Writer, ctx);
#pragma warning restore CS4014
        }
        return channel.Reader.ReadAllAsync(ctx);
    }


    private static async Task EnumerateFiles(IStorageSearch search, string query, ChannelWriter<IEnumerable<FileMetaData>> writer, CancellationToken ctx)
    {
        await foreach (var filesChunk in search.List(query, ctx))
        {
#pragma warning disable CS4014
            writer.WriteAsync(item: filesChunk, cancellationToken: ctx);
#pragma warning restore CS4014
        }
    }
}