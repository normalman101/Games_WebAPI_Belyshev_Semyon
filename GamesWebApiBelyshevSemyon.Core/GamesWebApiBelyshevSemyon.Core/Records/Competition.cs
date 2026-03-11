// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global

namespace WebAPI
{
    public record Competition()
    {
        public Guid Id { init; get; }
        public required string Name { init; get; }
        public required DateTime Date { init; get; }
        public required string Location { init; get; }
        public required string SportType { init; get; }
        public bool IsDeleted { get; init; } = false;
    }
}