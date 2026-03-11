using System.Text.Json;
// ReSharper disable SimplifyLinqExpressionUseAll

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable CheckNamespace

namespace WebAPI
{
    public static class Results
    {
        private const string _jsonResultsPath = "Files\\results.json";
        private static readonly string _jsonResults = File.ReadAllText(_jsonResultsPath);
        private static List<Result>? _results = JsonSerializer.Deserialize<List<Result>>(_jsonResults);

        public static List<Result>? GetResults()
        {
            return _results;
        }

        public static Result GetById(Guid id)
        {
            var foundResult = _results!.FirstOrDefault(result => result.Id == id);

            if (Utility.IsNull(foundResult)) Microsoft.AspNetCore.Http.Results.NotFound("Не найден результат");

            return foundResult!;
        }

        public static List<Result> GetByCompetitionId(Guid id)
        {
            var foundResults = _results!.Where(result => result.CompetitionId == id).ToList();

            if (foundResults.Count == 0)
                Microsoft.AspNetCore.Http.Results.NotFound("Не найдены результаты соревнования");
            //BUG: При поиске результатов по существующему идентификатору соревнований всю равно выдаётся ошибка 404
            return foundResults;
        }

        public static IResult Add(Result result)
        {
            if (!Competitions.GetCompetition()!.Any(competition => competition.Id == result.CompetitionId))
                return Microsoft.AspNetCore.Http.Results.BadRequest("Несуществующий идентификатор соревнования");
            if (Utility.IsEmpty(result.ParticipantName))
                return Microsoft.AspNetCore.Http.Results.BadRequest("Отсутствует имя");
            if (result.Place <= 0) return Microsoft.AspNetCore.Http.Results.BadRequest("Место не больше нуля");
            if (result.Score <= 0) return Microsoft.AspNetCore.Http.Results.BadRequest("Очки не больше нуля");

            var newResult = new Result
            {
                Id = Guid.NewGuid(),
                CompetitionId = result.CompetitionId,
                ParticipantName = result.ParticipantName,
                Place = result.Place,
                Score = result.Score
            };

            _results!.Add(newResult);

            Utility.UpdateDataFile(_jsonResultsPath, _results);

            return Microsoft.AspNetCore.Http.Results.Created();
        }

        public static IResult UpdateWithNew(Guid id, Result result)
        {
            var foundResult = _results!.FirstOrDefault(existingResult => existingResult.Id == id);

            if (Utility.IsNull(foundResult)) return Microsoft.AspNetCore.Http.Results.NotFound("Не найден результат");

            _results!.RemoveAll(existingResult => existingResult.Id == id);

            var newCompetitionId =
                Utility.IsEmpty(result.CompetitionId.ToString()) ? foundResult!.CompetitionId : result.CompetitionId;
            var newParticipantName = Utility.IsEmpty(result.ParticipantName)
                ? foundResult!.ParticipantName
                : result.ParticipantName;
            var newPlace = result.Place > 0 ? result.Place : foundResult!.Place;
            var newScore = result.Score > 0 ? result.Score : foundResult!.Score;

            var updatedResult = foundResult! with
            {
                CompetitionId = newCompetitionId,
                ParticipantName = newParticipantName,
                Place = newPlace,
                Score = newScore
            };

            _results.Add(updatedResult);

            Utility.UpdateDataFile(_jsonResultsPath, _results);

            return Microsoft.AspNetCore.Http.Results.Ok();
        }

        public static IResult DeleteById(Guid id)
        {
            var foundResult = _results!.FirstOrDefault(result => result.Id == id);

            if (Utility.IsNull(foundResult)) return Microsoft.AspNetCore.Http.Results.NotFound("Не найден результат");

            _results!.RemoveAll(existingResults => existingResults.Id == id);

            _results.Add(foundResult! with { IsDeleted = true });

            Utility.UpdateDataFile(_jsonResultsPath, _results);

            return Microsoft.AspNetCore.Http.Results.Ok();
        }
    }
}