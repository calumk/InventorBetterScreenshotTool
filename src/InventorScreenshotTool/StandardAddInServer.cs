using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Inventor;

namespace InventorScreenshotTool;

/// <summary>Same add-in GUID and command IDs as v0.1.2: upgrade in place.</summary>
[ComVisible(true)]
[Guid(AddinId)]
[ClassInterface(ClassInterfaceType.None)]
public sealed class StandardAddInServer : ApplicationAddInServer
{
    public const string AddinId = "9DB7878B-E3CC-456E-9769-75DA28AF669A";
    private const string ClientId = "{" + AddinId + "}";
    private const string CommandId = "InventorScreenshotTool.CapturePNG";
    private const string TabId = "InventorScreenshotTool.Tab";
    private const string PanelId = "InventorScreenshotTool.Panel";

    private Inventor.Application? _app;
    private ButtonDefinition? _captureButton;
    private Bitmap? _smallIcon;
    private Bitmap? _largeIcon;
    private readonly List<RibbonTab> _tabs = new();

    public object? Automation => null;

    public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
    {
        _app = addInSiteObject.Application;
        _smallIcon = RibbonIcons.Load(16);
        _largeIcon = RibbonIcons.Load(32);
        _captureButton = _app.CommandManager.ControlDefinitions.AddButtonDefinition(
            "Better Screenshot",
            CommandId,
            CommandTypesEnum.kQueryOnlyCmdType,
            ClientId,
            "Capture the active model as a white-background PNG with a preview.",
            "Autodesk Inventor Better Screenshot Tool - By CalumK",
            RibbonIcons.ToPictureDisp(_smallIcon),
            RibbonIcons.ToPictureDisp(_largeIcon));
        _captureButton.OnExecute += CaptureButton_OnExecute;

        AddRibbonTab("Part");
        AddRibbonTab("Assembly");
    }

    private void AddRibbonTab(string ribbonName)
    {
        if (_app is null || _captureButton is null) return;
        Ribbon ribbon = _app.UserInterfaceManager.Ribbons[ribbonName];
        RibbonTab tab = ribbon.RibbonTabs.Add("Better Screenshots", TabId, ClientId);
        RibbonPanel panel = tab.RibbonPanels.Add("Capture", PanelId, ClientId);
        panel.CommandControls.AddButton(_captureButton, true);
        _tabs.Add(tab);
    }

    private void CaptureButton_OnExecute(NameValueMap context)
    {
        if (_app is null) return;
        try
        {
            using var dialog = new ScreenshotDialog(_app);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open Better Screenshot Tool.\n\n{ex.Message}",
                "Better Screenshot Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void Deactivate()
    {
        if (_captureButton is not null)
            _captureButton.OnExecute -= CaptureButton_OnExecute;
        foreach (RibbonTab tab in _tabs)
        {
            try { tab.Delete(); }
            catch (COMException) { /* Inventor may have rebuilt its UI. */ }
        }
        _tabs.Clear();
        _captureButton = null;
        _smallIcon?.Dispose();
        _largeIcon?.Dispose();
        _smallIcon = _largeIcon = null;
        _app = null;
    }

    public void ExecuteCommand(int commandID) { /* Legacy API requirement. */ }
}
