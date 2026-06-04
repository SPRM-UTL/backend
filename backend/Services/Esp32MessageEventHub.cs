using System.Collections.Concurrent;
using System.Threading.Channels;
using backend.Models;

namespace backend.Services
{
    public class Esp32MessageEventHub
    {
        private readonly ConcurrentDictionary<Guid, Channel<MessageEvent>> _streams = new();

        public Guid Subscribe(out ChannelReader<MessageEvent> reader)
        {
            var id = Guid.NewGuid();
            var channel = Channel.CreateUnbounded<MessageEvent>();

            _streams[id] = channel;
            reader = channel.Reader;

            return id;
        }

        public void Unsubscribe(Guid id)
        {
            if (_streams.TryRemove(id, out var channel))
            {
                channel.Writer.TryComplete();
            }
        }

        public void Publish(MessageEvent message)
        {
            foreach (var stream in _streams.Values)
            {
                stream.Writer.TryWrite(message);
            }
        }
    }
}
