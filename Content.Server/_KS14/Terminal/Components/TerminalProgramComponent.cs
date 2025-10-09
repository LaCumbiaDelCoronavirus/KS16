
namespace Content.Server.KS14.Terminal;

/// <summary>Component for a terminal that runs Lua.</summary>
[RegisterComponent]
public sealed partial class TerminalProgramComponent : Component
{
    public TerminalProgram? Program;
}
