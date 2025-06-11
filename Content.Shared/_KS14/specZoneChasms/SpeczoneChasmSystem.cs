using Content.Shared.ActionBlocker;
using Content.Shared.Chasm;
using Content.Shared.Movement.Events;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared.KS14.SpecZones.Chasms;

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

        SubscribeLocalEvent<SpeczoneChasmComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<SpeczoneChasmComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
    }

    private void OnStepTriggered(Entity<SpeczoneChasmComponent> chasm, ref StepTriggeredOffEvent args)
    {
        QueueDel(args.Tripper);
    }

    private void OnStepTriggerAttempt(Entity<SpeczoneChasmComponent> chasm, ref StepTriggerAttemptEvent args) { args.Continue = true; }
}
