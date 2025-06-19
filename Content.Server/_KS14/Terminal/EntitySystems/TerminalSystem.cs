using System.Diagnostics.CodeAnalysis;
using Content.Shared.KS14.Terminal;

namespace Content.Server.KS14.Terminal;


public sealed class TerminalSystem : SharedTerminalSystem
{
    private EntityQuery<TerminalComponent> _terminalQuery;

    public override void Initialize()
    {
        base.Initialize();
        _terminalQuery = GetEntityQuery<TerminalComponent>();

        SubscribeLocalEvent<TerminalComponent, ComponentStartup>(TerminalInit);
        SubscribeNetworkEvent<TerminalFileMessage>(OnTerminalFileMessage);
    }

    private bool ValidateTerminal(NetEntity terminalNetEntity, [NotNullWhen(true)] out Entity<TerminalComponent>? terminal)
    {
        if (GetEntity(terminalNetEntity) is not { Valid: true } terminalUid)
        {
            terminal = null;
            return false;
        }

        if (!_terminalQuery.TryComp(terminalUid, out var terminalComponent))
        {
            terminal = null;
            return false;
        }

        terminal = (terminalUid, terminalComponent);
        return true;
    }


    private void TerminalInit(Entity<TerminalComponent> terminal, ref ComponentStartup args)
        => terminal.Comp.Program = new TerminalProgram(Log);

    public override void InitProgram(Entity<TerminalComponent> terminal)
    {
        var (_, terminalComponent) = terminal;
        terminalComponent.Program = new TerminalProgram(Log);
    }

    private void OnTerminalFileMessage(TerminalFileMessage args)
    {
        if (!ValidateTerminal(args.NetEntity, out var nTerminal) || nTerminal is not { } terminal)
            return;

        var (_, terminalComponent) = terminal;

        if (terminalComponent.Program is not { } program)
        {
            Log.Debug("Terminal has no program!");
            return;
        }

        var content = args.Content[..Math.Min(args.Content.Length, MaxTerminalFileSize)];
        Log.Debug($"Running terminal program: {content}");

        program.Run(content);
    }
}
