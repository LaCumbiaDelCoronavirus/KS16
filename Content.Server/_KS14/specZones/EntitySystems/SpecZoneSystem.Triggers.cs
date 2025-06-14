using Content.Shared.KS14.SpecZones.Systems;
using Content.Server.Explosion.EntitySystems;

namespace Content.Server.KS14.SpecZones;

public sealed partial class SpecZoneSystem : SharedSpecZoneSystem
{
    private void OnEndZoneTrigger(Entity<EndSpecialZoneOnTriggerComponent> triggerEnt, ref TriggerEvent triggerEv)
    {
        var zoneMapDictionary = GetZoneMapDictionary();
        var triggerEndingZoneId = triggerEnt.Comp.ZoneId;

        if (triggerEndingZoneId != null)
        {
            if (!zoneMapDictionary.TryGetValue(triggerEndingZoneId, out var activeZone))
                return;

            EndZone(activeZone);

            return;
        }

        var triggerEntMapUid = _transform.GetMap(triggerEnt.Owner);
        if (triggerEntMapUid != null && _specZoneQuery.TryGetComponent(triggerEntMapUid, out var triggerEntMapSpecZoneComponent))
            EndZone((triggerEntMapUid.Value, triggerEntMapSpecZoneComponent));
    }
}
