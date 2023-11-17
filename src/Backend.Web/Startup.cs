using Backend.Core.Domain;
using Backend.Core.Infra;
using Backend.Core.PubSub;
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
            builder.Services.AddSingleton(_ => Channel.CreateUnbounded<Person>(new UnboundedChannelOptions
            {
                SingleReader = true
            }));

            builder.Services.AddHostedService<BufferService>();
            builder.Services.AddSingleton<ConcurrentBag<Person>>();

            builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(appConfiguration.Redis));
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton(sp =>
            {
                var connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();

                return new BroadcastService(connectionMultiplexer, "people-channel");
            });

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
            }
        }
    }
}
