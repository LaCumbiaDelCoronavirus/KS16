namespace Content.Shared.SanabiFramework.PositionLogging;


// TODO: Maybe remove this if we don't end up needing it.
/// <summary>
/// Placeholder system that holds some data for serverside position logging.
/// </summary>
public abstract class SharedPositionLoggingSystem : EntitySystem
{

    /// <summary>The maximum number of positions that will be stored in a <see cref="PositionLoggingComponent.PositionQueue"/> at once.</summary>
    public const int QueueCap = 15;


    public override void Initialize()
    {
        base.Initialize();

        // TODO: write something here? Idfk.
    }
}
