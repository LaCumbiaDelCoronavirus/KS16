using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Shared.SanabiFramework.PositionLogging;


/// <summary>
/// Shared system that holds some data for serverside position logging.
/// This doesn't do anything on it's own, but exists to allow logging to be used on shared code.
/// </summary>
public abstract class SharedPositionLoggingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Tries to get the coords at the tick closest to the provided <paramref name="tick"/>,
    /// from the provided <paramref name="loggerEnt"/>'s <see cref="PositionLoggerComponent"/>'s queue,
    /// and sets the ref coordinates to it. If run on client, does not change anything.
    /// Does not do anything when not on server.
    /// </summary>
    /// <returns> Whether a position was found.
    public abstract bool PredictedGetPositionAtTick(Entity<PositionLoggerComponent?> loggerEnt, GameTick tick, [NotNullWhen(true)] ref EntityCoordinates? coordinates);
}
