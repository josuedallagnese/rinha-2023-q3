using System.Text.Json;
using StackExchange.Redis;

namespace Backend.Core.PubSub
{
    public class BroadcastService
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        private readonly RedisChannel _channel;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ISubscriber _subscriber;

        public BroadcastService(IConnectionMultiplexer connectionMultiplexer, string channel)
        {
            _channel = new RedisChannel(channel, RedisChannel.PatternMode.Auto);
            _connectionMultiplexer = connectionMultiplexer;
            _subscriber = _connectionMultiplexer.GetSubscriber();
        }

        public async Task Propagate<T>(T value)
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonSerializerOptions);

            await _subscriber.PublishAsync(_channel, serializedValue);
        }

        public async Task Receive<T>(Action<T> action)
        {
            await _subscriber.SubscribeAsync(_channel, (channel, value) =>
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(value.ToString(), _jsonSerializerOptions) ?? default;

                action.Invoke(deserializedValue);
            });
        }
    }
}
