using System.Diagnostics.CodeAnalysis;
using Content.Shared.SanabiFramework.PositionLogging;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client.SanabiFramework.PositionLogging;

public sealed class PositionLoggingSystem : SharedPositionLoggingSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    /// <inheritdoc/>
    public override bool ResolvePredictedPositionAtTick(Entity<PositionLoggerComponent?> loggerEnt, GameTick tick, [NotNullWhen(true)] ref EntityCoordinates coordinates)
        => false;

    /// <inheritdoc/>
    public override bool ResolvePredictedPositionAtTick(NetEntity loggerNetUid, GameTick tick, [NotNullWhen(true)] ref EntityCoordinates coordinates)
        => false;
}
