using Content.Shared.DeviceLinking;
using Content.Shared.Verbs;

namespace Content.Shared.KS14.Terminal;


public abstract class SharedTerminalSystem : EntitySystem
{
    [Dependency] protected readonly SharedDeviceLinkSystem LinkSystem = default!;
    [Dependency] protected readonly SharedAppearanceSystem AppearanceSystem = default!;

    public const int MaxTerminalFileSize = 10000;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerminalComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<TerminalComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerbs);
    }

    private void OnInit(Entity<TerminalComponent> terminal, ref ComponentInit args)
    {
        var (uid, terminalComponent) = terminal;

        LinkSystem.EnsureSinkPorts(uid, terminalComponent.InPort);
        LinkSystem.EnsureSourcePorts(uid, terminalComponent.OutPort);
    }

    private void OnGetAlternativeVerbs(Entity<TerminalComponent> terminal, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanComplexInteract || !args.CanAccess)
            return;

        AlternativeVerb restartVerb = new()
        {
            Act = () => InitProgram(terminal),
            Text = "Restart"
        };

        AlternativeVerb runVerb = new()
        {
            Act = () => UploadDialog(terminal),
            Text = "Upload Lua file"
        };

        args.Verbs.Add(restartVerb);
        args.Verbs.Add(runVerb);
    }

    /// <summary>Starts a new program, or creates a new one if one already exists on this terminal.</summary>
    public virtual void InitProgram(Entity<TerminalComponent> terminal) { }

    /// <summary>Opens a file dialog for uploading and running a lua file on this terminal.</summary>
    public virtual void UploadDialog(Entity<TerminalComponent> terminal) { }
}
