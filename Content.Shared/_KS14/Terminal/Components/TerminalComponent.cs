using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.KS14.Terminal;


/// <summary>
/// Component for a terminal that runs Lua.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TerminalComponent : Component
{
    /// <summary>The maximum number of signal source ports that this terminal can have, assuming it has the necessary components.</summary>
    [DataField(), ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<SinkPortPrototype> InPort = "TerminalIn";

    /// <summary>The maximum number of signal sink ports that this terminal can have, assuming it has the necessary components.</summary>
    [DataField(), ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<SourcePortPrototype> OutPort = "TerminalOut";

    /// <summary>The server-side Lua program being run on this terminal.</summary>
    [NonSerialized]
    public SharedTerminalProgram? Program;
}

[Serializable, NetSerializable]
public sealed class TerminalFileMessage : EntityEventArgs
{
    public NetEntity NetEntity;
    public string Content;

    public TerminalFileMessage(NetEntity netEntity, string content)
    {
        NetEntity = netEntity;
        Content = content;
    }
}
