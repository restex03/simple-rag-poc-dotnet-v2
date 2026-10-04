using Microsoft.AspNetCore.Builder;
using Rag.Core.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("healthz");

app.MapPost("/documents", () =>
{
    return Results.Ok(new
    {
        message = "Document ingestion endpoint",
    });
});

app.MapPost("/search", () =>
{
   return Results.Ok(new
   {
       message = "Vector Search endpoint",
   });
});

app.MapPost("/ask", () =>
{
    return Results.Ok(new
    {
        message = "RAG question answering endpoint",
    });
});



app.Run();
