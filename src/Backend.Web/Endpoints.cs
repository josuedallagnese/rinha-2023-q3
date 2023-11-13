using Backend.Core;
using Backend.Core.PubSub;
using Backend.Web.Domain;
using Backend.Web.Infra;
using Backend.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Channels;

namespace Backend.Web
{
    public static class Endpoints
    {
        public static async Task ConfigureEndpoints(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            await MapPost(app);
            MapGet(app);
            MapGetAll(app);
            MapCount(app);
            MapThreadCount(app);
        }

        private static void MapThreadCount(WebApplication app)
        {
            app.MapGet("/thread-count", (HttpContext http) =>
            {
                var threadCount = new ThreadCount();
                ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
                threadCount.MinWorkerThreads = workerThreads;
                threadCount.MinCompletionPortThreads = completionPortThreads;

                ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
                threadCount.MaxWorkerThreads = workerThreads;
                threadCount.MaxCompletionPortThreads = completionPortThreads;

                return threadCount;
            }).Produces<ThreadCount>();
        }

        private static void MapCount(WebApplication app)
        {
            app.MapGet("/contagem-pessoas", async (HttpContext http, ReadRepository repository) =>
            {
                var total = await repository.Count();

                return total;
            }).Produces<long>();
        }

        private static void MapGetAll(WebApplication app)
        {
            app.MapGet("/pessoas", async (HttpContext http, ReadRepository repository, string t) =>
            {
                if (string.IsNullOrEmpty(t))
                    return Results.BadRequest();

                var pessoas = await repository.GetAll(t);

                return Results.Ok(pessoas);

            }).Produces<IEnumerable<PersonRequest>>();
        }

        private static void MapGet(WebApplication app)
        {
            app.MapGet("/pessoas/{id}", async (HttpContext http, ReadRepository repository, IMemoryCache cache, Guid id) =>
            {
                var personRequest = cache.Get<PersonRequest>(id);
                if (personRequest != null)
                {
                    return Results.Ok(personRequest);
                }

                personRequest = await repository.Get(id);
                if (personRequest == null)
                    return Results.NotFound();

                return Results.Ok(personRequest);

            }).Produces<PersonRequest>();
        }

        private static async Task MapPost(WebApplication app)
        {
            var broadcastService = app.Services.GetRequiredService<IBroadcastService>();
            var cache = app.Services.GetRequiredService<IMemoryCache>();

            Action<PersonRequest> updateCache = (request) =>
            {
                // A chave usando o id verifica a existência no endpoint de pessoas/{id}
                cache.Set(request.Id, request);

                // A chave usando apelido verifica a existência neste mesmo endpoint.
                cache.Set(request.NickName, request);
            };

            app.MapPost("/pessoas", async (HttpContext http, Channel<Person> channel, NewPersonRequest request, AppConfiguration appConfiguration) =>
            {
                // Não atende aos criterios de validação basica? 422
                if (request == null || !request.Validate())
                    return Results.UnprocessableEntity();

                // Já existe em cache? 422
                var cacheValue = cache.Get<PersonRequest>(request.Apelido);
                if (cacheValue != null)
                    return Results.UnprocessableEntity();

                var entity = new Person(
                    request.Apelido,
                    request.Nome,
                    request.DataNascimento,
                    request.Stack);

                cacheValue = new PersonRequest()
                {
                    Id = entity.Id,
                    NickName = request.Apelido,
                    BirthDate = request.Nascimento,
                    Name = request.Nome,
                    Stack = request.Stack
                };

                await broadcastService.Propagate(cacheValue);

                await channel.Writer.WriteAsync(entity);

                updateCache(cacheValue);

                // Gera uma compensação para a replicação entre instâncias
                await Task.Delay(appConfiguration.CacheReplicationCompensation);

                return Results.Created($"/pessoas/{entity.Id}", request);

            }).Produces<NewPersonRequest>();

            await broadcastService.Receive(updateCache);
        }
    }
}
