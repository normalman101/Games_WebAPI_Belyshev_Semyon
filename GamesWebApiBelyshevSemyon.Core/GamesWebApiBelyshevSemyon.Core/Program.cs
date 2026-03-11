using WebAPI;

// ReSharper disable UseSymbolAlias

// ReSharper disable RedundantArgumentDefaultValue

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

const string version = "v1";

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint($"/openapi/{version}.json", version); });
}

app.UseHttpsRedirection();

#region Методы соревнований

app.MapGet("/api/competitions", Competitions.GetCompetition);

app.MapGet("/api/competitions/{id:guid}", Competitions.GetById).Produces<Competition>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/competitions", Competitions.Add).Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status201Created);

app.MapPut("/api/competitions/{id:guid}", Competitions.UpdateWithNew).Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status201Created);

app.MapDelete("/api/competitions/{id:guid}", Competitions.DeleteById).Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status200OK);

#endregion

#region Результаты

app.MapGet("/api/results", WebAPI.Results.GetResults);

app.MapGet("/api/results/{id:guid}", WebAPI.Results.GetById).Produces(StatusCodes.Status404NotFound);

app.MapGet("/api/competitions/{competitionId:guid}/results", WebAPI.Results.GetByCompetitionId)
    .Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/results", WebAPI.Results.Add).Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status201Created);

app.MapPut("/api/results/{id:guid}", WebAPI.Results.UpdateWithNew).Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status200OK);

app.MapDelete("/api/results/{id:guid}", WebAPI.Results.DeleteById).Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status200OK);

#endregion

app.Run();