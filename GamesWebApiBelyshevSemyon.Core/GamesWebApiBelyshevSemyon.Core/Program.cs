using System.Text.Json;
using GamesWebApiBelyshevSemyon.Core;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });
}

app.UseHttpsRedirection();

#region Общие сущности

var jsonCompetitions = File.ReadAllText("Files\\competitions.json");
var jsonResults = File.ReadAllText("Files\\results.json");

var competitions = JsonSerializer.Deserialize<List<Competition>>(jsonCompetitions);
var results = JsonSerializer.Deserialize<List<Result>>(jsonResults);

void UpdateCompetitions()
{
    var option = new JsonSerializerOptions { WriteIndented = true };
    File.WriteAllText("Files\\competitions.json", JsonSerializer.Serialize(competitions, option));
}

void UpdateResults()
{
    var option = new JsonSerializerOptions { WriteIndented = true };
    File.WriteAllText("Files\\results.json", JsonSerializer.Serialize(results, option));
}

#endregion

#region Методы соревнований

app.MapGet("/api/competitions", () => competitions);

app.MapGet("/api/competitions/{id:guid}", (Guid id) =>
{
    var foundCompetition = competitions!.FirstOrDefault(competition => competition.Id == id);

    if (foundCompetition == null) Results.NotFound("Не найдено соревнование");

    return foundCompetition;
}).Produces<Competition>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/competitions", (Competition competition) =>
{
    if (competition.Name.Length == 0) return Results.BadRequest("Отсутствует имя");
    if (competition.Date == DateTime.MinValue) return Results.BadRequest("Отсутствует дата");
    if (competition.Location.Length == 0) return Results.BadRequest("Отсутствует локация");
    if (competition.SportType.Length == 0) return Results.BadRequest("Отсутствует вид спорта");

    var newCompetition = competition with { Id = Guid.NewGuid(), IsDeleted = false };

    competitions!.Add(newCompetition);

    UpdateCompetitions();

    return Results.Created();
}).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status201Created);

app.MapPut("/api/competitions/{id:guid}", (Guid id, Competition competition) =>
{
    var foundCompetition = competitions!.FirstOrDefault(existingCompetition => existingCompetition.Id == id);

    if (foundCompetition == null) return Results.NotFound();

    competitions!.RemoveAll(existingCompetition => existingCompetition.Id == id);

    var newName = competition.Name.Length == 0 ? foundCompetition.Name : competition.Name;
    var newDate = competition.Date == DateTime.MinValue ? foundCompetition.Date : competition.Date;
    var newLocation = competition.Location.Length == 0 ? foundCompetition.Location : competition.Location;
    var newSportType = competition.SportType.Length == 0 ? foundCompetition.SportType : competition.SportType;

    var updatedCompetition = competition with
    {
        Name = newName,
        Date = newDate,
        Location = newLocation,
        SportType = newSportType
    };

    competitions!.Add(updatedCompetition);

    UpdateCompetitions();

    return Results.Created();
}).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status201Created);

app.MapDelete("/api/competitions/{id:guid}", (Guid id) =>
{
    var foundCompetition = competitions!.FirstOrDefault(competition => competition.Id == id);

    if (foundCompetition == null) return Results.NotFound("Не найдено соревнование");

    competitions!.RemoveAll(existingCompetition => existingCompetition.Id == id);

    competitions.Add(foundCompetition! with { IsDeleted = true });

    UpdateCompetitions();

    return Results.Ok();
}).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status200OK);

#endregion

#region Результаты

app.MapGet("/api/results", () => results);

app.MapGet("/api/results/{id:guid}", (Guid id) =>
{
    var foundResult = results!.FirstOrDefault(result => result.Id == id);

    if (foundResult != null) return foundResult;

    Results.NotFound("Не найден результат");
    return null;
}).Produces(StatusCodes.Status404NotFound);

app.MapGet("/api/competitions/{competitionId:guid}/results", (Guid competitionId) =>
{
    var foundResults = results!.Where(result => result.CompetitionId == competitionId);

    if (foundResults.Any()) return foundResults;

    Results.NotFound("Не найдены результаты соревнования");
    return null;
}).Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/results", (Result result) =>
{
    if (competitions!.Any(competition => competition.Id != result.Id))
        return Results.BadRequest("Несуществующий идентификатор соревнования");
    if (result.ParticipantName.Length == 0) return Results.BadRequest("Отсутствует имя");
    if (result.Place <= 0) return Results.BadRequest("Место не больше нуля");
    if (result.Score <= 0) return Results.BadRequest("Очки не больше нуля");

    var newResult = new Result
    {
        Id = Guid.NewGuid(),
        CompetitionId = result.CompetitionId,
        ParticipantName = result.ParticipantName,
        Place = result.Place,
        Score = result.Score
    };

    results!.Add(result);

    UpdateResults();

    return Results.Created();
}).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status201Created);

app.MapPut("/api/results/{id}", (Guid id, Result result) =>
{
    var foundResult = results!.FirstOrDefault(existingResult => existingResult.Id == id);

    if (foundResult == null) return Results.NotFound("Не найден результат");

    results!.RemoveAll(existingResult => existingResult.Id == id);

    var newCompetitionId =
        result.CompetitionId.ToString().Length == 0 ? foundResult!.CompetitionId : result.CompetitionId;
    var newParticipantName = result.ParticipantName.Length == 0 ? foundResult!.ParticipantName : result.ParticipantName;
    var newPlace = result.Place > 0 ? result.Place : foundResult!.Place;
    var newScore = result.Score > 0 ? result.Score : foundResult!.Score;

    var updatedResult = result with
    {
        CompetitionId = newCompetitionId,
        ParticipantName = newParticipantName,
        Place = newPlace,
        Score = newScore
    };

    results!.Add(updatedResult);

    UpdateResults();

    return Results.Ok();
}).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status200OK);

app.MapDelete("/api/results/{id:guid}", (Guid id) =>
{
    var foundResult = results!.FirstOrDefault(result => result.Id == id);

    if (foundResult == null) return Results.NotFound("Не найден результат");

    results!.RemoveAll(existingResults => existingResults.Id == id);
    
    results!.Add(foundResult! with { IsDeleted = true });
    
    UpdateResults();

    return Results.Ok();
}).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status200OK);

#endregion

app.Run();