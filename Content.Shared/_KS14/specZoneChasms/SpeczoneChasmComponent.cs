using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.KS14.SpecZones.Chasms;

/// <summary>
///     Marks a component that will cause entities to fall into them on a step trigger activation
/// </summary>
[NetworkedComponent, RegisterComponent, Access(typeof(SpecZoneChasmSystem))]
public sealed partial class SpeczoneChasmComponent : Component
{
    /// <summary>
    ///     Sound that should be played when an entity falls into the chasm
    /// </summary>
    [DataField("fallingSound")]
    public SoundSpecifier FallingSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");
}
