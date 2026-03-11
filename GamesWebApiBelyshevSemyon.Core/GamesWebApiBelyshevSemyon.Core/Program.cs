using System.Text.Json;
using GamesWebApiBelyshevSemyon.Core;

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

#region Общие сущности

const string jsonCompetitionsPath = "Files\\competitions.json";
const string jsonResultsPath = "Files\\results.json";

var jsonCompetitions = File.ReadAllText(jsonCompetitionsPath);
var jsonResults = File.ReadAllText(jsonResultsPath);

var competitions = JsonSerializer.Deserialize<List<Competition>>(jsonCompetitions);
var results = JsonSerializer.Deserialize<List<Result>>(jsonResults);

bool IsEmpty(string variable)
{
    return (variable.Length == 0);
}

bool IsNull<T>(T variable)
{
    return (variable == null);
}

void UpdateDataFile<T>(string filePath, T data) where T : class
{
    var option = new JsonSerializerOptions { WriteIndented = true };
    File.WriteAllText(filePath, JsonSerializer.Serialize(data, option));
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
    if (IsEmpty(competition.Name)) return Results.BadRequest("Отсутствует имя");
    if (competition.Date == DateTime.MinValue) return Results.BadRequest("Отсутствует дата");
    if (IsEmpty(competition.Location)) return Results.BadRequest("Отсутствует локация");
    if (IsEmpty(competition.SportType)) return Results.BadRequest("Отсутствует вид спорта");

    var newCompetition = competition with { Id = Guid.NewGuid(), IsDeleted = false };

    competitions!.Add(newCompetition);

    UpdateDataFile(jsonCompetitionsPath, competitions);

    return Results.Created();
}).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status201Created);

app.MapPut("/api/competitions/{id:guid}", (Guid id, Competition competition) =>
{
    var foundCompetition = competitions!.FirstOrDefault(existingCompetition => existingCompetition.Id == id);

    if (IsNull(foundCompetition)) return Results.NotFound();

    competitions!.RemoveAll(existingCompetition => existingCompetition.Id == id);

    var newName = IsEmpty(competition.Name) ? foundCompetition!.Name : competition.Name;
    var newDate = competition.Date == DateTime.MinValue ? foundCompetition!.Date : competition.Date;
    var newLocation = IsEmpty(competition.Location) ? foundCompetition!.Location : competition.Location;
    var newSportType = IsEmpty(competition.SportType) ? foundCompetition!.SportType : competition.SportType;

    var updatedCompetition = competition with
    {
        Name = newName,
        Date = newDate,
        Location = newLocation,
        SportType = newSportType
    };

    competitions.Add(updatedCompetition);

    UpdateDataFile(jsonCompetitionsPath, competitions);

    return Results.Created();
}).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status201Created);

app.MapDelete("/api/competitions/{id:guid}", (Guid id) =>
{
    var foundCompetition = competitions!.FirstOrDefault(competition => competition.Id == id);

    if (IsNull(foundCompetition)) return Results.NotFound("Не найдено соревнование");

    competitions!.RemoveAll(existingCompetition => existingCompetition.Id == id);

    competitions.Add(foundCompetition! with { IsDeleted = true });

    UpdateDataFile(jsonCompetitionsPath, competitions);

    return Results.Ok();
}).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status200OK);

#endregion

#region Результаты

app.MapGet("/api/results", () => results);

app.MapGet("/api/results/{id:guid}", (Guid id) =>
{
    var foundResult = results!.FirstOrDefault(result => result.Id == id);

    if (IsNull(foundResult)) return foundResult;

    Results.NotFound("Не найден результат");
    return null;
}).Produces(StatusCodes.Status404NotFound);

app.MapGet("/api/competitions/{competitionId:guid}/results", (Guid competitionId) =>
{
    var foundResults = results!.Where(result => result.CompetitionId == competitionId).ToList();

    if (foundResults.Count == 0) return foundResults;

    Results.NotFound("Не найдены результаты соревнования");
    return null;
}).Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/results", (Result result) =>
{
    if (competitions!.Any(competition => competition.Id != result.Id))
        return Results.BadRequest("Несуществующий идентификатор соревнования");
    if (IsEmpty(result.ParticipantName)) return Results.BadRequest("Отсутствует имя");
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

    results!.Add(newResult);

    UpdateDataFile(jsonResultsPath, results);

    return Results.Created();
}).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status201Created);

app.MapPut("/api/results/{id}", (Guid id, Result result) =>
{
    var foundResult = results!.FirstOrDefault(existingResult => existingResult.Id == id);

    if (IsNull(foundResult)) return Results.NotFound("Не найден результат");

    results!.RemoveAll(existingResult => existingResult.Id == id);

    var newCompetitionId =
        IsEmpty(result.CompetitionId.ToString()) ? foundResult!.CompetitionId : result.CompetitionId;
    var newParticipantName = IsEmpty(result.ParticipantName) ? foundResult!.ParticipantName : result.ParticipantName;
    var newPlace = result.Place > 0 ? result.Place : foundResult!.Place;
    var newScore = result.Score > 0 ? result.Score : foundResult!.Score;

    var updatedResult = result with
    {
        CompetitionId = newCompetitionId,
        ParticipantName = newParticipantName,
        Place = newPlace,
        Score = newScore
    };

    results.Add(updatedResult);

    UpdateDataFile(jsonResultsPath, results);

    return Results.Ok();
}).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status200OK);

app.MapDelete("/api/results/{id:guid}", (Guid id) =>
{
    var foundResult = results!.FirstOrDefault(result => result.Id == id);

    if (IsNull(foundResult)) return Results.NotFound("Не найден результат");

    results!.RemoveAll(existingResults => existingResults.Id == id);

    results.Add(foundResult! with { IsDeleted = true });

    UpdateDataFile(jsonResultsPath, results);

    return Results.Ok();
}).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status200OK);

#endregion

app.Run();