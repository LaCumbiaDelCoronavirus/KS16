using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Server.KS14.SpecZones.Chasms;

/// <summary>
///     Marks a component that will cause entities to fall into them on a step trigger activation
/// </summary>
[RegisterComponent, Access(typeof(SpecZoneChasmSystem))]
public sealed partial class SpeczonePortalComponent : Component
{
    /// <summary>
    ///     Sound that should be played when an entity falls into the chasm
    /// </summary>
    [DataField("fallingSound")]
    public SoundSpecifier FallingSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");

    /// <summary>
    /// ID of the zone to go to. If empty, ejects the user out of the zone.
    /// </summary>
    [DataField]
    public string? ZoneId = null;
}
