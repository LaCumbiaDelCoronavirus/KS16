using Content.Shared.KS14.Terminal;
using MoonSharp;
using MoonSharp.Interpreter;

namespace Content.Server.KS14.Terminal;


/// <summary>
/// Wrapper for a Lua program, with a few methods.
/// </summary>
sealed public class TerminalProgram : SharedTerminalProgram
{
    private ISawmill _sawmill;

    public Script Lua;
    public Action<ScriptRuntimeException>? ExceptionAct;

    public TerminalProgram(ISawmill log)
    {
        _sawmill = log;
        Lua = new Script(CoreModules.Preset_SoftSandbox);
        Lua.Options.DebugPrint = output => _sawmill.Debug($"LTerminal: {output}");
    }

    public override void SetGlobal(string name, object? value)
        => Lua.Globals[name] = value;

    /// <summary>
    /// Runs a Lua string, and catches any <see cref="ScriptRuntimeException"/>s.
    /// </summary>
    /// <param name="exec">The Lua string to execute.</param>
    public override void Run(string exec)
    {
        try { Lua.DoString(exec); }
        catch (ScriptRuntimeException ex)
        {
            if (ExceptionAct != null)
                ExceptionAct(ex);
        }

    }
}
