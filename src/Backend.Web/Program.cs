using Backend.Web;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

var app = builder.Build();

await app.ConfigureEndpoints();

app.Run();
