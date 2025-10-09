using System.Runtime.CompilerServices;
using Content.Shared.KS14.Terminal;
using MoonSharp;
using MoonSharp.Interpreter;

namespace Content.Server.KS14.Terminal;


/// <summary>
/// Wrapper for a Lua program that throttles the amount of lua operations per tick.
/// </summary>
sealed public class TerminalProgram : SharedTerminalProgram
{
    public Script Lua { get; private set; }

    private Coroutine? _wrapperCoroutine;
    public CoroutineState? WrappedCoroutineState => _wrapperCoroutine?.State;

    private ISawmill _sawmill;
    public int YieldCounter = 5;

    /// <summary>Optional delegate that is called when the Lua program throws a <see cref="ScriptRuntimeException"/>.</summary>
    public Action<ScriptRuntimeException>? RuntimeExceptionAct;
    /// <summary>Optional delegate that is called when the Lua program throws a <see cref="SyntaxErrorException"/>.</summary>
    public Action<SyntaxErrorException>? SyntaxExceptionAct;

    public TerminalProgram(ISawmill log)
    {
        Lua = new Script(CoreModules.Preset_SoftSandbox);
        _sawmill = log;

        Lua.Options.DebugPrint = output => _sawmill.Debug($"LTerminal: {output}");
    }

    public void Dispose()
    {
        RuntimeExceptionAct = null;
        SyntaxExceptionAct = null;

        Lua?.Globals.Clear();

        GC.SuppressFinalize(this);
    }

    public void SetGlobal(string name, object? value)
        => Lua.Globals[name] = value;

    /// <summary>Continues execution of the program after it has been yielded.</summary>
    public void Continue()
    {
        if (_wrapperCoroutine == null)
            return;

        if (_wrapperCoroutine.State == CoroutineState.NotStarted || _wrapperCoroutine.State == CoroutineState.Suspended)
            _wrapperCoroutine.Resume();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnLuaRuntimeException(ScriptRuntimeException ex, ref bool successful)
    {
        if (RuntimeExceptionAct != null)
            RuntimeExceptionAct(ex);

        successful = false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnLuaSyntaxException(SyntaxErrorException ex, ref bool successful)
    {
        if (SyntaxExceptionAct != null)
            SyntaxExceptionAct(ex);

        successful = false;
    }


    /// <summary>
    /// Runs a Lua string, and catches any <see cref="ScriptRuntimeException"/>s, calling the delegate <see cref="ExceptionAct"/> with it.
    ///     It is loaded in a singleton throttled coroutine, with a maximum of <see cref="YieldCounter"/> number of operations before being yielded,
    ///     and having to be resumed by <see cref="Continue"/>.
    /// </summary>
    /// <remarks>Only one string can be loaded in a coroutine at a time.</remarks>
    /// <returns>Whether the operation was successful.</returns>
    /// <param name="exec">The Lua string to execute.</param>
    public bool Load(string exec)
    {
        bool successful = true;

        try
        {
            _wrapperCoroutine = Lua.CreateCoroutine(Lua.LoadString(exec)).Coroutine;
            _wrapperCoroutine.AutoYieldCounter = YieldCounter;
        }
        catch (ScriptRuntimeException ex) { OnLuaRuntimeException(ex, ref successful); }
        catch (SyntaxErrorException ex) { OnLuaSyntaxException(ex, ref successful); }

        return successful;
    }

    /// <summary>
    /// Runs a Lua string, and catches any <see cref="ScriptRuntimeException"/>s, calling the delegate <see cref="ExceptionAct"/> with it.
    ///     This is not wrapped in a coroutine, unlike <see cref="Load"/>.
    /// </summary>
    /// <returns>Whether the operation was successful.</returns>
    /// <param name="exec">The Lua string to execute.</param>
    public bool Run(string exec)
    {
        bool successful = true;

        try { Lua.DoString(exec); }
        catch (ScriptRuntimeException ex) { OnLuaRuntimeException(ex, ref successful); }
        catch (SyntaxErrorException ex) { OnLuaSyntaxException(ex, ref successful); }

        return successful;
    }
}
