using Content.Shared.ActionBlocker;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Server.KS14.SpecZones.Chasms;

/// <summary>
///     Handles making entities fall into chasms when stepped on.
/// </summary>
public sealed class SpecZoneChasmSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeczonePortalComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<SpeczonePortalComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
    }

    private void OnStepTriggered(Entity<SpeczonePortalComponent> chasm, ref StepTriggeredOffEvent args)
    {
        QueueDel(args.Tripper);
    }

    private void OnStepTriggerAttempt(Entity<SpeczonePortalComponent> chasm, ref StepTriggerAttemptEvent args) { args.Continue = true; }
}
