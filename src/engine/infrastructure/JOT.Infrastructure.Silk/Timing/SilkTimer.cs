namespace JOT.Infrastructure.Silk.Timing;

/// <summary>
/// Silk.NET implementation of <see cref="ITimer"/>.
/// </summary>
internal sealed class SilkTimer : ITimer
{
    /// <summary>
    /// Gets the time elapsed since the previous update, in seconds.
    /// </summary>
    public double DeltaTime { get; internal set; }

    /// <summary>
    /// Gets the total elapsed time since the application started, in seconds.
    /// </summary>
    public double TotalTime { get; internal set; }
}
