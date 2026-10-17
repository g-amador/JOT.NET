namespace JOT.Infrastructure.Silk.Timing;

/// <summary>
/// Provides timing information for the application loop.
/// </summary>
public interface ITimer
{
    /// <summary>
    /// Gets the time elapsed since the previous update, in seconds.
    /// </summary>
    double DeltaTime { get; }

    /// <summary>
    /// Gets the total elapsed time since the application started, in seconds.
    /// </summary>
    double TotalTime { get; }
}