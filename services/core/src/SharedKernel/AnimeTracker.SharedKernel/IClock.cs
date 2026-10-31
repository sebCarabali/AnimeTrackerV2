namespace AnimeTracker.SharedKernel;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
