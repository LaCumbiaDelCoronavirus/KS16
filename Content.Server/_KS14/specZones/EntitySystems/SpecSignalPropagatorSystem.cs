using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Server.DeviceNetwork;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Robust.Shared.Prototypes;

namespace Content.Server.KS14.SpecZones;


/// <summary>
/// Handles the control of output based on the input and enable ports.
/// </summary>
public sealed class SignalPropagatorSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _deviceSignalSystem = default!;
    [Dependency] private readonly AutoLinkSystem _autoLinkSystem = default!;

    private int _unaccumulatedPropagators = 0;

    // How many seconds from compinit does this
    private const float PropagatorUpdateTimer = 15f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignalPropagatorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SignalPropagatorComponent, SignalReceivedEvent>(OnSignalReceived);

        SubscribeLocalEvent<SignalPropagatorComponent, EntityUnpausedEvent>(OnPropagatorUnpaused);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_unaccumulatedPropagators <= 0)
            return;

        var signalPropagatorQuery = EntityQueryEnumerator<SignalPropagatorComponent>();
        while (signalPropagatorQuery.MoveNext(out var uid, out var propagatorComponent))
        {
            propagatorComponent.UpdateAccumulator += frameTime;

            if (propagatorComponent.FullyUpdated == true)
                continue;

            if (propagatorComponent.UpdateAccumulator < PropagatorUpdateTimer)
                continue;

            var propagator = (uid, propagatorComponent);
            EnsurePropagatorLinked(propagator);
            UpdateOutput(propagator);

            _unaccumulatedPropagators--;
            propagatorComponent.FullyUpdated = true;
        }
    }

    private void OnPropagatorUnpaused(Entity<SignalPropagatorComponent> ent, ref EntityUnpausedEvent args)
    {
        EnsurePropagatorLinked(ent);
    }

    private void EnsurePropagatorLinked(Entity<SignalPropagatorComponent> ent)
    {
        var (entUid, propagatorComponent) = ent;

        _deviceSignalSystem.EnsureSourcePorts(entUid, propagatorComponent.HighSourcePorts.ToArray());
        _deviceSignalSystem.EnsureSourcePorts(entUid, propagatorComponent.LowSourcePorts.ToArray());

        _deviceSignalSystem.EnsureSinkPorts(entUid, propagatorComponent.EnablePort, propagatorComponent.DisablePort, propagatorComponent.TogglePort);

        _autoLinkSystem.AutoLink((entUid, null));
    }

    private void SignalPortList(Entity<SignalPropagatorComponent, DeviceLinkSourceComponent?> ent, List<ProtoId<SourcePortPrototype>> portList, bool signal)
    {
        var (entUid, propagatorComponent, linkSourceComp) = ent;
        portList.ForEach(port => _deviceSignalSystem.SendSignal(ent, port, signal, linkSourceComp));
    }

    private void OnMapInit(Entity<SignalPropagatorComponent> ent, ref MapInitEvent args)
    {
        EnsurePropagatorLinked(ent);
        UpdateOutput(ent);

        _unaccumulatedPropagators++;
    }

    private void OnSignalReceived(Entity<SignalPropagatorComponent> ent, ref SignalReceivedEvent args)
    {
        var (entUid, propagatorComp) = ent;
        UpdateOutput(ent);

        var state = SignalState.Momentary;
        args.Data?.TryGetValue(DeviceNetworkConstants.LogicState, out state);

        if (args.Port == propagatorComp.EnablePort)
        {
            if (state == SignalState.High || state == SignalState.Momentary)
                if (propagatorComp.Status == false)
                    SetOutput(ent, true);
        }
        else if (args.Port == propagatorComp.DisablePort)
        {
            if (state == SignalState.High || state == SignalState.Momentary)
                if (propagatorComp.Status == true)
                    SetOutput(ent, false);
        }
        else if (args.Port == propagatorComp.TogglePort)
        {
            if (state == SignalState.Momentary) // not high just momentary
                SetOutput(ent, propagatorComp.Status ^ true);
        }
    }

    public void UpdateOutput(Entity<SignalPropagatorComponent, DeviceLinkSourceComponent?> ent)
    {
        var (_, propagatorComp, _) = ent;

        if (!Resolve(ent, ref ent.Comp2))
            return;

        if (propagatorComp.Status)
        {
            //SignalPortList(ent, propagatorComp.LowSourcePorts, false);
            SignalPortList(ent, propagatorComp.HighSourcePorts, true);
        }
        else
        {
            //SignalPortList(ent, propagatorComp.HighSourcePorts, false);
            SignalPortList(ent, propagatorComp.LowSourcePorts, true);
        }


    }

    private void SetOutput(Entity<SignalPropagatorComponent> ent, bool newStatus)
    {
        ent.Comp.Status = newStatus;
        UpdateOutput(ent);
    }
}
