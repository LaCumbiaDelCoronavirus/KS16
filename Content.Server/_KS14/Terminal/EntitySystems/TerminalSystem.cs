using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Content.Shared.KS14.Terminal;
using MoonSharp.Interpreter;

namespace Content.Server.KS14.Terminal;


public sealed class TerminalSystem : SharedTerminalSystem
{
    private EntityQuery<TerminalComponent> _terminalQuery;
    private EntityQuery<TerminalProgramComponent> _terminalProgramQuery;

    public override void Initialize()
    {
        base.Initialize();
        _terminalQuery = GetEntityQuery<TerminalComponent>();
        _terminalProgramQuery = GetEntityQuery<TerminalProgramComponent>();

        SubscribeLocalEvent<TerminalProgramComponent, ComponentStartup>(TerminalProgramInit);
        SubscribeNetworkEvent<TerminalFileMessage>(OnTerminalFileMessage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var terminalProgramEnum = EntityQueryEnumerator<TerminalProgramComponent>();
        while (terminalProgramEnum.MoveNext(out _, out var terminalComponent))
        {
            if (terminalComponent.Program is not { } program)
                continue;

            program.Continue();
        }
    }

    /// <summary>Validates a terminal for net purposes.</summary>
    private bool ValidateTerminal(NetEntity terminalNetEntity, [NotNullWhen(true)] out Entity<TerminalProgramComponent>? terminal)
    {
        if (GetEntity(terminalNetEntity) is not { Valid: true } terminalUid ||
            !_terminalProgramQuery.TryComp(terminalUid, out var terminalProgramComponent))
        {
            terminal = null;
            return false;
        }

        terminal = (terminalUid, terminalProgramComponent);
        return true;
    }

    /// <summary>Validates a terminal for net purposes.</summary>
    private bool ValidateTerminal(NetEntity terminalNetEntity, [NotNullWhen(true)] out Entity<TerminalComponent, TerminalProgramComponent>? terminal)
    {
        if (GetEntity(terminalNetEntity) is not { Valid: true } terminalUid ||
            !_terminalQuery.TryComp(terminalUid, out var terminalComponent) ||
            !_terminalProgramQuery.TryComp(terminalUid, out var terminalProgramComponent))
        {
            terminal = null;
            return false;
        }

        terminal = (terminalUid, terminalComponent, terminalProgramComponent);
        return true;
    }

    private void UpdateAppearance(Entity<TerminalComponent> terminal)
    {
        var (_, terminalComponent) = terminal;

        AppearanceSystem.SetData(terminal, TerminalVisuals.Light, terminalComponent.State);
    }

    private void TerminalProgramInit(Entity<TerminalProgramComponent> terminal, ref ComponentStartup args)
        => terminal.Comp.Program = new TerminalProgram(Log);

    private void OnTerminalRuntimeException(Entity<TerminalComponent>? maybeTerminal, ScriptRuntimeException exception)
    {
        if (maybeTerminal is not { } terminal)
            return;

        var (_, terminalComponent) = terminal;

        Log.Debug($"Processing terminal runtime exception! {exception.DecoratedMessage}");

        terminalComponent.State = TerminalState.Stopped;
        UpdateAppearance(terminal);
    }

    private void OnTerminalSyntaxException(Entity<TerminalComponent>? maybeTerminal, SyntaxErrorException exception)
    {
        if (maybeTerminal is not { } terminal)
            return;

        var (_, terminalComponent) = terminal;

        Log.Debug($"Processing terminal syntax exception! {exception.DecoratedMessage}");

        terminalComponent.State = TerminalState.Yielding;
        UpdateAppearance(terminal);
    }

    public override void InitProgram(Entity<TerminalComponent> terminal)
    {
        var (terminalUid, terminalComponent) = terminal;

        if (!_terminalProgramQuery.TryComp(terminalUid, out var terminalProgramComponent)) { Log.Error("Could not resolve TerminalProgramComponent on terminal!"); return; }

        TerminalProgram program = new(Log);

        program.RuntimeExceptionAct = exception => OnTerminalRuntimeException(terminal, exception);
        program.SyntaxExceptionAct = exception => OnTerminalSyntaxException(terminal, exception);

        terminalProgramComponent.Program = program;
        terminalComponent.State = TerminalState.Working;
        UpdateAppearance(terminal);
    }

    private void OnTerminalFileMessage(TerminalFileMessage args)
    {
        if (!ValidateTerminal(args.NetEntity, out Entity<TerminalComponent, TerminalProgramComponent>? nTerminal) || nTerminal is not { } terminal)
            return;

        var (_, terminalComponent, terminalProgramComponent) = terminal;
        if (terminalProgramComponent.Program is not TerminalProgram program)
        {
            Log.Debug("Terminal has no program!");
            return;
        }

        var content = args.Content[..Math.Min(args.Content.Length, MaxTerminalFileSize)];
        Log.Debug($"Loading terminal program: {content}");

        program.Run(content);

        terminalComponent.State = TerminalState.Working;
        UpdateAppearance(terminal);
    }
}
