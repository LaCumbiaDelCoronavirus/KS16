using Content.Shared.KS14.SpecZones.Systems;
using Content.Shared.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Server.DoAfter;

namespace Content.Server.KS14.SpecZones;

public sealed partial class SpecZoneSystem : SharedSpecZoneSystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    private void OnKeyUseInhand(Entity<SpecZoneKeyComponent> key, ref UseInHandEvent args)
    {
        var user = args.User;
        var keyDoAfter = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(5), new SpecZoneKeyDoAfterEvent(), key.Owner)
        {
            DistanceThreshold = 1f,
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        if (_doAfter.TryStartDoAfter(keyDoAfter))
            _popupSystem.PopupEntity(Loc.GetString("speczone-key-doafter-start", ("user", _identity.GetEntityIdentity(user))), user, PopupType.Medium);

        FindExitPositions();
    }

    private void OnBadDecision(Entity<SpecZoneKeyComponent> key, ref SpecZoneKeyDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var user = args.User;
        var targetZoneId = key.Comp.ZoneId ?? GetRandomZoneId();

        if (!GetZoneMapDictionary().TryGetValue(targetZoneId, out var targetZone))
            return;

        TrySetZoneAwake(targetZone, true);

        var zoneEntrancePosition = GetRandomZoneEntrance(targetZoneId);
        if (zoneEntrancePosition == null)
            return;

        _transform.TryGetMapOrGridCoordinates(user, out var useCoordinates);

        // you're fucked now
        InsertIntoZone(user, targetZone, zoneEntrancePosition.Value);
        EjectFromZone(key.Owner, targetZone);

        if (_mindSystem.TryGetMind(user, out var mindId, out _) && _mindSystem.TryGetSession(mindId, out var mind))
            _audio.PlayGlobal(ZoneEnterSoundSpec, mind);

        if (useCoordinates != null)
            _popupSystem.PopupCoordinates(Loc.GetString("speczone-key-doafter-end", ("user", _identity.GetEntityIdentity(user))), useCoordinates.Value, PopupType.LargeCaution);
    }
}
