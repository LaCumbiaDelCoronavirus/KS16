using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Content.Shared.SanabiFramework.PositionLogging;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server.SanabiFramework.PositionLogging;


/// <summary>
/// System that handles logging the position of entities for the last <see cref="SharedPositionLoggingSystem.QueueCap"/> ticks,
/// via <see cref="PositionLoggerComponent"/>.
/// </summary>
public sealed class PositionLoggingSystem : SharedPositionLoggingSystem
{
    [Dependency] IGameTiming _gameTiming = default!;

    private EntityQuery<PositionLoggerComponent> _loggerQuery;

    private static readonly FieldInfo QueueArray = typeof(Queue<>).GetField("_array", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo QueueHead = typeof(Queue<>).GetField("_head", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public override void Initialize()
    {
        base.Initialize();
        _loggerQuery = GetEntityQuery<PositionLoggerComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // We don't check for where the entity has a transform comp because fukyou.
        var loggerEnum = EntityQueryEnumerator<PositionLoggerComponent>();
        while (loggerEnum.MoveNext(out var uid, out var loggerComponent))
        {
            var loggerQueue = loggerComponent.PositionQueue;

            // It should always be below QueueCap.
            if (loggerQueue.Count + 1 >= QueueCap)
                loggerQueue.Dequeue();

            loggerQueue.Enqueue(Transform(uid).Coordinates);
        }
    }

    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="uid"/> has no <see cref="PositionLoggerComponent"/> component, or the
    /// entity does not exist.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Queue<EntityCoordinates> GetQueue(EntityUid uid) => _loggerQuery.GetComponent(uid).PositionQueue;

    /// <summary>
    /// Tries to return the coords at the tick closest to the provided <paramref name="tick"/>,
    /// from the provided <paramref name="loggerComponent"/>'s queue.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the queue of <paramref name="loggerComponent"/> is empty.</exception>
    public static EntityCoordinates GetPositionAtTick(PositionLoggerComponent loggerComponent, GameTick tick)
    {
        // TODO: Figure out if i should cast something here to int
        var tickValue = tick.Value;
        var tickDifference = tickValue - loggerComponent.LastRecordedTick;

        var logQueue = loggerComponent.PositionQueue;
        // If the provided tick is older than the oldest tick we have recorded, just return the oldest tick.
        if (tickDifference >= SharedPositionLoggingSystem.QueueCap)
            return logQueue.Peek();

        // tickdifference < 0 means provided tick is too new, so just use the latest one we have since I don't feel making this predict the future
        var tickIndex = Math.Max(tickDifference, 0);

        EntityCoordinates[] queueArray = (EntityCoordinates[]) QueueArray.GetValue(logQueue)!;
        int head = (int) QueueHead.GetValue(logQueue)!;

        var index = logQueue.Count - 1 - tickIndex;
        return queueArray[(head + index) % queueArray.Length];
    }

    /// <summary>
    /// Tries to return the coords at the tick closest to the provided <paramref name="tick"/>,
    /// from the provided <paramref name="uid"/>'s <see cref="PositionLoggerComponent"/>'s queue.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="uid"/> has no <see cref="PositionLoggerComponent"/> component, or the
    /// entity does not exist.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityCoordinates GetPositionAtTick(EntityUid uid, GameTick tick) => GetPositionAtTick(_loggerQuery.GetComponent(uid), tick);

    /// <summary>
    /// Tries to return the coords at the tick closest to the provided <paramref name="tick"/>,
    /// from the provided <paramref name="uid"/>'s <see cref="PositionLoggerComponent"/>'s queue.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetPositionAtTick(EntityUid uid, GameTick tick, out EntityCoordinates coordinates)
    {
        if (!_loggerQuery.TryGetComponent(uid, out var loggerComponent))
        {
            coordinates = EntityCoordinates.Invalid;
            return false;
        }

        coordinates = GetPositionAtTick(loggerComponent, tick);
        return true;
    }
}
