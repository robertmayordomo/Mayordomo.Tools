using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Mayordomo.Tools.Channels;

public class ChannelQueueFactory
{
    private readonly ConcurrentDictionary<string, object> _queues = new();

    private static string GetKey<T>(string? explicitName)
    {
        var typeName = typeof(T).FullName ?? typeof(T).Name;
        var queueName = explicitName ?? typeof(T).Name;
        return $"{typeName}|{queueName}";
    }

    public ChannelWriter<T> GetProducer<T>(
        string? name = null,
        Action<ChannelQueueBuilder<T>>? configure = null)
    {
        var key = GetKey<T>(name);
        var queue = GetOrCreate(key, configure);
        return queue.Writer;
    }

    public ChannelReader<T> GetConsumer<T>(
        string? name = null,
        Action<ChannelQueueBuilder<T>>? configure = null)
    {
        var key = GetKey<T>(name);
        var queue = GetOrCreate(key, configure);
        return queue.Reader;
    }

    public ChannelQueue<T> GetQueue<T>(
        string? name = null,
        Action<ChannelQueueBuilder<T>>? configure = null)
    {
        var key = GetKey<T>(name);
        return GetOrCreate(key, configure);
    }

    private ChannelQueue<T> GetOrCreate<T>(string key, Action<ChannelQueueBuilder<T>>? configure)
    {
        if (_queues.TryGetValue(key, out var existing) && existing is ChannelQueue<T> q)
            return q;

        var builder = new ChannelQueueBuilder<T>();
        configure?.Invoke(builder);

        if (configure == null)
            builder.Unbounded()
                .SingleReader(false)
                .SingleWriter(false);

        var newQueue = builder.Build();
        var actual = (ChannelQueue<T>)_queues.GetOrAdd(key, newQueue);
        return actual;
    }

    public class ChannelQueueBuilder<T>
    {
        private bool _allowSynchronousContinuations;
        private bool _bounded;
        private int _capacity = 100;
        private BoundedChannelFullMode _fullMode = BoundedChannelFullMode.Wait;
        private bool _singleReader;
        private bool _singleWriter;

        public ChannelQueueBuilder<T> Bounded(int capacity = 100,
            BoundedChannelFullMode fullMode = BoundedChannelFullMode.Wait)
        {
            _bounded = true;
            _capacity = capacity;
            _fullMode = fullMode;
            return this;
        }

        public ChannelQueueBuilder<T> Unbounded()
        {
            _bounded = false;
            return this;
        }

        public ChannelQueueBuilder<T> SingleReader(bool value = true)
        {
            _singleReader = value;
            return this;
        }

        public ChannelQueueBuilder<T> SingleWriter(bool value = true)
        {
            _singleWriter = value;
            return this;
        }

        public ChannelQueueBuilder<T> AllowSynchronousContinuations(bool value = true)
        {
            _allowSynchronousContinuations = value;
            return this;
        }

        internal ChannelQueue<T> Build()
        {
            if (_bounded)
            {
                var boundedChannelOptions = new BoundedChannelOptions(_capacity)
                {
                    SingleReader = _singleReader,
                    SingleWriter = _singleWriter,
                    FullMode = _fullMode,
                    AllowSynchronousContinuations = _allowSynchronousContinuations
                };
                return new ChannelQueue<T>(Channel.CreateBounded<T>(boundedChannelOptions));
            }


            var options = new UnboundedChannelOptions
            {
                SingleReader = _singleReader,
                SingleWriter = _singleWriter,
                AllowSynchronousContinuations = _allowSynchronousContinuations
            };

            return new ChannelQueue<T>(Channel.CreateUnbounded<T>(options));
        }
    }
}