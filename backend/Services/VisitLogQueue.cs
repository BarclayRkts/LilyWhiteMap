using System.Threading.Channels;
using LilyWhiteMap.Api.Models;

namespace LilyWhiteMap.Api.Services;

/// <summary>
/// In-memory, backpressure-aware queue that decouples request handling from database writes.
/// Writers (the middleware) only enqueue; a background service drains and persists in batches.
/// </summary>
public sealed class VisitLogQueue
{
    private readonly Channel<VisitLogEntry> _channel = Channel.CreateBounded<VisitLogEntry>(
        new BoundedChannelOptions(capacity: 5_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });

    public ChannelReader<VisitLogEntry> Reader => _channel.Reader;

    /// <summary>Non-blocking enqueue; safe to call from the request pipeline without awaiting.</summary>
    public bool TryEnqueue(VisitLogEntry entry) => _channel.Writer.TryWrite(entry);
}
