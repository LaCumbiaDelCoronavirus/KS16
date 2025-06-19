namespace Content.Shared.KS14.Terminal;


/// <summary>
/// Wrapper for a Lua program with a few methods. This one is shared because client is sandboxed,
///     and can't import MoonSharp.
/// </summary>
public abstract class SharedTerminalProgram
{
    public abstract void SetGlobal(string name, object? value);

    /// <summary>
    /// Runs a Lua string, and catches any <see cref="ScriptRuntimeException"/>s.
    /// </summary>
    /// <param name="exec">The Lua string to execute.</param>
    public abstract void Run(string exec);
}
