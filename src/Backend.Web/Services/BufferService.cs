using System.Collections.Concurrent;
using System.Threading.Channels;
using Backend.Web.Domain;
using Backend.Web.Infra;

namespace Backend.Web.Services
{
    public class BufferService : BackgroundService
    {
        private readonly Channel<Person> _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AppConfiguration _appConfiguration;
        private readonly ConcurrentBag<Person> _buffer;
        private readonly ILogger<BufferService> _logger;

        public BufferService(
            Channel<Person> channel,
            IServiceScopeFactory serviceScopeFactory,
            AppConfiguration appConfiguration,
            ConcurrentBag<Person> buffer,
            ILogger<BufferService> logger)
        {
            _channel = channel;
            _serviceScopeFactory = serviceScopeFactory;
            _appConfiguration = appConfiguration;
            _buffer = buffer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bufferSize = _appConfiguration.BufferSize;

            await foreach (var item in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                _buffer.Add(item);

                if (_buffer.Count < bufferSize)
                    continue;

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var repository = scope.ServiceProvider.GetRequiredService<Repository>();

                    await repository.Insert(_buffer);

                    _buffer.Clear();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Processamento do buffer");
                }
            }
        }
    }
}
