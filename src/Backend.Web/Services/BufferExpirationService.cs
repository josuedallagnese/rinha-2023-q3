using System.Collections.Concurrent;
using Backend.Web.Domain;
using Backend.Web.Infra;

namespace Backend.Web.Services
{
    public class BufferExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AppConfiguration _appConfiguration;
        private readonly ILogger<BufferService> _logger;

        public BufferExpirationService(
            IServiceScopeFactory serviceScopeFactory,
            AppConfiguration appConfiguration,
            ILogger<BufferService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _appConfiguration = appConfiguration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bufferExpiration = _appConfiguration.BufferExpiration;

            using var timer = new PeriodicTimer(bufferExpiration);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var buffer = scope.ServiceProvider.GetRequiredService<ConcurrentBag<Person>>();

                    if (buffer.IsEmpty)
                        continue;

                    var repository = scope.ServiceProvider.GetRequiredService<Repository>();

                    await repository.Insert(buffer);

                    buffer.Clear();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Controle expiração buffer");
                }
            }
        }
    }
}
