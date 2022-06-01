using System.Threading.Channels;

namespace Storage.Common;

public class SearchAggregator : IStorageSearch
{
    private readonly IEnumerable<IStorageSearch> _searchServices;

    public SearchAggregator(IEnumerable<IStorageSearch> searchServices)
    {
        _searchServices = searchServices;
    }


    public IAsyncEnumerable<IEnumerable<FileMetaData>> List(string name, CancellationToken ctx)
    {
        var channel = Channel.CreateUnbounded<IEnumerable<FileMetaData>>();
        var enumerationTasks = new List<Task>();
        foreach (var searchService in _searchServices)
        {
#pragma warning disable CS4014
            var task = EnumerateFilesTask(searchService, name, channel.Writer, ctx);
            enumerationTasks.Add(task);
#pragma warning restore CS4014
        }

        Task.WhenAll(enumerationTasks).ContinueWith(_ => channel.Writer.Complete(), ctx);

        return channel.Reader.ReadAllAsync(ctx);
    }


    private static async Task EnumerateFilesTask(IStorageSearch search, string query,
        ChannelWriter<IEnumerable<FileMetaData>> writer, CancellationToken ctx)
    {
        try
        {
            await foreach (var filesChunk in search.List(query, ctx))
            {
                await writer.WriteAsync(item: filesChunk, cancellationToken: ctx);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}