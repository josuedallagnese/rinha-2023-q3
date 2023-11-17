using Backend.Core;
using Backend.Core.Domain;
using Backend.Core.Infra;
using Backend.Core.Models;
using Backend.Core.PubSub;
using Backend.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
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

            MapPost(app);
            MapGet(app);
            MapGetAll(app);
            MapCount(app);
            MapThreadCount(app);

            await SubscribeAddPersonEvent(app);
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
            app.MapGet("/contagem-pessoas", async (HttpContext http, ConcurrentBag<Person> buffer, Repository repository) =>
            {
                if (!buffer.IsEmpty)
                {
                    await repository.Insert(buffer);

                    buffer.Clear();
                }

                return await repository.Count();
            }).Produces<long>();
        }

        private static void MapGetAll(WebApplication app)
        {
            app.MapGet("/pessoas", async (HttpContext http, Repository repository, string t) =>
            {
                if (string.IsNullOrEmpty(t))
                    return Results.BadRequest();

                var response = await repository.GetAll(t);

                return Results.Ok(response);

            }).Produces<IEnumerable<PersonRequest>>();
        }

        private static void MapGet(WebApplication app)
        {
            app.MapGet("/pessoas/{id}", async (HttpContext http, Repository repository, IMemoryCache cache, Guid id, AppConfiguration appConfiguration) =>
            {
                // primeira tentativa de verificação no cache
                var response = cache.Get<PersonRequest>(id);
                if (response != null)
                    return Results.Ok(response);

                // se ainda não sincronizou, gera a compensação e espera
                await Task.Delay(appConfiguration.CacheReplicationCompensation);

                // segunda tentativa de verificação no cache
                response = cache.Get<PersonRequest>(id);
                if (response != null)
                    return Results.Ok(response);

                response = await repository.Get(id);
                if (response == null)
                    return Results.NotFound();

                return Results.Ok(response);

            }).Produces<PersonRequest>();
        }

        private static void MapPost(WebApplication app)
        {
            app.MapPost("/pessoas", async (HttpContext http, Channel<Person> channel, NewPersonRequest request, IMemoryCache cache, BroadcastService broadcastService) =>
            {
                // Não atende aos criterios de validação basica? 422
                if (request == null || !request.Validate())
                    return Results.UnprocessableEntity();

                // Já existe em cache? 422
                var response = cache.Get<PersonRequest>(request.Apelido);
                if (response != null)
                    return Results.UnprocessableEntity();

                var entity = new Person(
                    request.Apelido,
                    request.Nome,
                    request.DataNascimento,
                    request.Stack);

                await channel.Writer.WriteAsync(entity);

                response = new PersonRequest()
                {
                    Id = entity.Id,
                    NickName = request.Apelido,
                    BirthDate = request.Nascimento,
                    Name = request.Nome,
                    Stack = request.Stack
                };

                cache.Set(response.Id, response);
                cache.Set(response.NickName, response);

                await broadcastService.Propagate(response);

                return Results.Created($"/pessoas/{entity.Id}", response);

            }).Produces<PersonRequest>();
        }

        private static async Task SubscribeAddPersonEvent(WebApplication app)
        {
            var broadcastService = app.Services.GetRequiredService<BroadcastService>();
            var cache = app.Services.GetRequiredService<IMemoryCache>();

            await broadcastService.Receive<PersonRequest>((personRequest) =>
            {
                cache.Set(personRequest.Id, personRequest);
                cache.Set(personRequest.NickName, personRequest);
            });
        }
    }
}
