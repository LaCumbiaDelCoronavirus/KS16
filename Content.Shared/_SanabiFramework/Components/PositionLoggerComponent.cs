using Content.Shared.SanabiFramework.PositionLogging;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Shared.SanabiFramework.PositionLogging;


/// <summary>
/// Component that stores a queue of the last <see cref="PositionLoggingSystem.QueueCap"/> positions of an entity.
/// </summary>
[RegisterComponent]
public sealed partial class PositionLoggerComponent : Component
{
    /// <summary>
    /// A queue with the maximum length of <see cref="PositionLoggingSystem.QueueCap"/>.
    /// Stores the position of the component's entity, by the last several ticks.
    /// </summary>
    public Queue<EntityCoordinates> PositionQueue = new();

    /// <summary>
    /// The last tick at which <see cref="PositionQueue"/> was updated.
    /// </summary>
    public int LastRecordedTick = (int) GameTick.Zero.Value;
}
