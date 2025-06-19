using System.IO;
using Content.Shared.KS14.Terminal;
using Robust.Client.UserInterface;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.KS14.Terminal;


public sealed class TerminalSystem : SharedTerminalSystem
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IFileDialogManager _dialogManager = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    private bool _isDialogOpen = false;

    public async override void UploadDialog(Entity<TerminalComponent> terminal)
    {
        if (_isDialogOpen)
            return;

        _isDialogOpen = true;
        var filters = new FileDialogFilters(new FileDialogFilters.Group("txt"));
        await using var file = await _dialogManager.OpenFile(filters);

        _isDialogOpen = false;
        if (file == null)
            return;

        using var reader = new StreamReader(file);
        string content = await reader.ReadToEndAsync();

        RaiseNetworkEvent(new TerminalFileMessage(GetNetEntity(terminal.Owner), content[..Math.Min(content.Length, MaxTerminalFileSize)]));
    }
}
