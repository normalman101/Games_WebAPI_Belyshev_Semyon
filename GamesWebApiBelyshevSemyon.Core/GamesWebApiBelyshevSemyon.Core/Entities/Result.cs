namespace GamesWebApiBelyshevSemyon.Core;

public record Result()
{
    public Guid Id { init; get; }
    public required Guid CompetitionId { init; get; }
    public required string ParticipantName { init; get; }
    public required uint Place { init; get; }
    public required uint Score { init; get; }
    public bool IsDeleted { init; get; } = false;
};