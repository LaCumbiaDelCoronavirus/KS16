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
    /// <summary>The maximum number of positions that will be stored in a <see cref="PositionLoggerComponent.PositionQueue"/> at once.</summary>
    public const int QueueCap = 15;

    private static readonly FieldInfo QueueArray = typeof(Queue<>).GetField("_array", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo QueueHead = typeof(Queue<>).GetField("_head", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // We don't check for where the entity has a transform comp because fukyou.
        var loggerEnum = EntityQueryEnumerator<PositionLoggerComponent>();
        while (loggerEnum.MoveNext(out var uid, out var loggerComponent))
        {
            var loggerQueue = loggerComponent.PositionQueue;

            // It should always be below QueueCap, rather than making it 16 and then trimming excess
            if (loggerQueue.Count + 1 >= QueueCap)
                loggerQueue.Dequeue();

            loggerQueue.Enqueue(Transform(uid).Coordinates);
        }
    }

    /// <summary>
    /// Tries to return the coords at the tick closest to the provided <paramref name="tick"/>,
    /// from the provided <paramref name="loggerComponent"/>'s queue. Doesn't do anything on client.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the queue of <paramref name="loggerComponent"/> is empty.</exception>
    private static EntityCoordinates GetPositionAtTick(PositionLoggerComponent loggerComponent, GameTick tick)
    {
        // TODO: Figure out if i should cast something here to int
        var tickValue = tick.Value;
        var tickDifference = tickValue - loggerComponent.LastRecordedTick;

        var logQueue = loggerComponent.PositionQueue;
        // If the provided tick is older than the oldest tick we have recorded, just return the oldest tick.
        if (tickDifference >= QueueCap)
            return logQueue.Peek();

        // tickdifference < 0 means provided tick is too new, so just use the latest one we have since I don't feel making this predict the future
        var tickIndex = Math.Max(tickDifference, 0);

        EntityCoordinates[] queueArray = (EntityCoordinates[]) QueueArray.GetValue(logQueue)!;
        int head = (int) QueueHead.GetValue(logQueue)!;

        var index = logQueue.Count - 1 - tickIndex;
        return queueArray[(head + index) % queueArray.Length];
    }

    /// <summary>
    /// Returns the coords at the tick closest to the provided <paramref name="tick"/>,
    /// from the provided <paramref name="loggerEnt"/>'s <see cref="PositionLoggerComponent"/>'s queue.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityCoordinates GetPositionAtTick(Entity<PositionLoggerComponent?> loggerEnt, GameTick tick)
    {
        if (!Resolve(loggerEnt, ref loggerEnt.Comp))
            return EntityCoordinates.Invalid;

        return GetPositionAtTick(loggerEnt.Comp, tick);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool PredictedGetPositionAtTick(Entity<PositionLoggerComponent?> loggerEnt, GameTick tick, [NotNullWhen(true)] ref EntityCoordinates? coordinates)
    {
        if (!Resolve(loggerEnt, ref loggerEnt.Comp))
            return false;

        coordinates = GetPositionAtTick(loggerEnt.Comp, tick);
        return true;
    }
}
