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
    /// Resolves a set of coords at the tick closest to the provided <paramref name="tick"/>,
    /// from the provided <paramref name="loggerEnt"/>'s <see cref="PositionLoggerComponent"/>'s queue,
    /// Does not do anything and only returns false when not run on server.
    /// </summary>
    /// <returns>Whether a position was found.</returns>
    public abstract bool ResolvePredictedPositionAtTick(Entity<PositionLoggerComponent?> loggerEnt, GameTick tick, [NotNullWhen(true)] ref EntityCoordinates coordinates);

    /// <summary>
    /// Networked entity variant of ResolvePredictedPositionAtTick. Takes a NetEntity,
    /// instead of an Entity.
    /// </summary>
    /// <returns>Whether a position was found.</returns>
    public abstract bool ResolvePredictedPositionAtTick(NetEntity loggerNetUid, GameTick tick, [NotNullWhen(true)] ref EntityCoordinates coordinates);
}
