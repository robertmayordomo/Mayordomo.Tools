using System.Threading.Channels;

namespace Mayordomo.Tools.Channels;

/// <summary>
///     Holds both writer and reader sides of a channel.
///     Usually you only give one side to each component via DI/factory.
/// </summary>
public class ChannelQueue<T>
{
    private readonly Channel<T> _channel;

    internal ChannelQueue(Channel<T> channel)
    {
        _channel = channel;
    }

    public ChannelWriter<T> Writer => _channel.Writer;
    public ChannelReader<T> Reader => _channel.Reader;

    public static ChannelQueue<T> CreateUnbounded(
        bool singleReader = false,
        bool singleWriter = false,
        bool allowSynchronousContinuations = false)
    {
        var options = new UnboundedChannelOptions
        {
            SingleReader = singleReader,
            SingleWriter = singleWriter,
            AllowSynchronousContinuations = allowSynchronousContinuations
        };
        return new ChannelQueue<T>(Channel.CreateUnbounded<T>(options));
    }

    public static ChannelQueue<T> CreateBounded(
        int capacity,
        bool singleReader = false,
        bool singleWriter = false,
        BoundedChannelFullMode fullMode = BoundedChannelFullMode.Wait,
        bool allowSynchronousContinuations = false)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            SingleReader = singleReader,
            SingleWriter = singleWriter,
            FullMode = fullMode,
            AllowSynchronousContinuations = allowSynchronousContinuations
        };
        return new ChannelQueue<T>(Channel.CreateBounded<T>(options));
    }
}