using Content.Shared.KS14.Terminal;

namespace Content.Server.KS14.Terminal;


public abstract class TerminalSystem : SharedTerminalSystem
{
    private EntityQuery<TerminalComponent> _terminalQuery;

    public override void Initialize()
    {
        base.Initialize();
        _terminalQuery = GetEntityQuery<TerminalComponent>();

        SubscribeNetworkEvent<TerminalFileMessage>(OnTerminalFileMessage);
    }

    public override void InitProgram(Entity<TerminalComponent> terminal)
    {
        var (uid, terminalComponent) = terminal;
        terminalComponent.Program = new TerminalProgram(Log);
    }

    private void OnTerminalFileMessage(TerminalFileMessage args)
    {
        if (GetEntity(args.NetEntity) is not { Valid: true } terminalUid)
            return;

        if (!_terminalQuery.TryComp(terminalUid, out var terminalComponent))
            return;

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
