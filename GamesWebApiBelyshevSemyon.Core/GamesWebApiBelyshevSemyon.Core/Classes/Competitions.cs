using System.Text.Json;
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable CheckNamespace

namespace WebAPI
{
    public static class Competitions
    {
        private const string _jsonCompetitionsPath = "Files\\competitions.json";
        private static readonly string _jsonCompetitions = File.ReadAllText(_jsonCompetitionsPath);
        private static List<Competition>? _competitions =
            JsonSerializer.Deserialize<List<Competition>>(_jsonCompetitions);

        public static List<Competition>? GetCompetition()
        {
            return _competitions;
        }

        public static Competition GetById(Guid id)
        {
            var foundCompetition = _competitions!.FirstOrDefault(competition => competition.Id == id);

            if (Utility.IsNull(foundCompetition)) Microsoft.AspNetCore.Http.Results.NotFound("Не найдено соревнование");

            return foundCompetition!;
        }

        public static IResult Add(Competition competition)
        {
            if (Utility.IsEmpty(competition.Name)) return Microsoft.AspNetCore.Http.Results.BadRequest("Отсутствует имя");
            if (competition.Date == DateTime.MinValue) return Microsoft.AspNetCore.Http.Results.BadRequest("Отсутствует дата");
            if (Utility.IsEmpty(competition.Location)) return Microsoft.AspNetCore.Http.Results.BadRequest("Отсутствует локация");
            if (Utility.IsEmpty(competition.SportType)) return Microsoft.AspNetCore.Http.Results.BadRequest("Отсутствует вид спорта");

            var newCompetition = competition with { Id = Guid.NewGuid(), IsDeleted = false };

            _competitions!.Add(newCompetition);

            Utility.UpdateDataFile(_jsonCompetitionsPath, _competitions);

            return Microsoft.AspNetCore.Http.Results.Created();
        }

        public static IResult UpdateWithNew(Guid id, Competition competition)
        {
            var foundCompetition = _competitions!.FirstOrDefault(existingCompetition => existingCompetition.Id == id);

            if (Utility.IsNull(foundCompetition)) return Microsoft.AspNetCore.Http.Results.NotFound();

            _competitions!.RemoveAll(existingCompetition => existingCompetition.Id == id);

            var newName = Utility.IsEmpty(competition.Name) ? foundCompetition!.Name : competition.Name;
            var newDate = competition.Date == DateTime.MinValue ? foundCompetition!.Date : competition.Date;
            var newLocation = Utility.IsEmpty(competition.Location) ? foundCompetition!.Location : competition.Location;
            var newSportType = Utility.IsEmpty(competition.SportType)
                ? foundCompetition!.SportType
                : competition.SportType;

            var updatedCompetition = foundCompetition! with
            {
                Name = newName,
                Date = newDate,
                Location = newLocation,
                SportType = newSportType
            };

            _competitions.Add(updatedCompetition);

            Utility.UpdateDataFile(_jsonCompetitionsPath, _competitions);

            return Microsoft.AspNetCore.Http.Results.Created();
        }

        public static IResult DeleteById(Guid id)
        {
            var foundCompetition = _competitions!.FirstOrDefault(competition => competition.Id == id);

            if (Utility.IsNull(foundCompetition)) return Microsoft.AspNetCore.Http.Results.NotFound("Не найдено соревнование");

            _competitions!.RemoveAll(existingCompetition => existingCompetition.Id == id);

            _competitions.Add(foundCompetition! with { IsDeleted = true });

            Utility.UpdateDataFile(_jsonCompetitionsPath, _competitions);

            return Microsoft.AspNetCore.Http.Results.Ok();
        }
    }
}