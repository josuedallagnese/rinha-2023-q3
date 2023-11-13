using Backend.Core.PubSub;
using Backend.Web.Domain;
using Backend.Web.Infra;
using Backend.Web.Services;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Backend.Web
{
    public static class Startup
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            builder.Configuration.AddEnvironmentVariables();

            var appConfiguration = new AppConfiguration(builder.Configuration);

            builder.Services.AddSingleton(appConfiguration);

            builder.Services.AddNpgsqlDataSource(appConfiguration.Npgsql);

            builder.Services.AddScoped<Repository>();
            builder.Services.AddScoped<ReadRepository>();

            builder.Services.AddSingleton(_ => Channel.CreateUnbounded<Person>(new UnboundedChannelOptions
            {
                SingleReader = true
            }));

            builder.Services.AddHostedService<BufferService>();
            builder.Services.AddHostedService<BufferExpirationService>();
            builder.Services.AddSingleton<ConcurrentBag<Person>>();

            builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(appConfiguration.Redis));
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<IBroadcastService>(sp =>
            {
                var connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();

                return new RedisBroadcastService(connectionMultiplexer, "people-channel");
            });

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
            }
        }
    }
}
