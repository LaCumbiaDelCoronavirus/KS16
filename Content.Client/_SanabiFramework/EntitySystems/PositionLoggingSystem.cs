using System.Diagnostics.CodeAnalysis;
using Content.Shared.SanabiFramework.PositionLogging;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client.SanabiFramework.PositionLogging;

public abstract class PositionLoggingSystem : SharedPositionLoggingSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    /// <inheritdoc/>
    public override bool PredictedGetPositionAtTick(Entity<PositionLoggerComponent?> loggerEnt, GameTick tick, [NotNullWhen(true)] ref EntityCoordinates? coordinates)
        => false;
}
