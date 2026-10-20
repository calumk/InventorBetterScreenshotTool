using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace InventorScreenshotTool;

internal sealed class ScreenshotDialog : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(242, 245, 250);
    private static readonly Color Ink = Color.FromArgb(29, 41, 57);
    private static readonly Color Muted = Color.FromArgb(103, 113, 132);
    private static readonly Color Accent = Color.FromArgb(37, 99, 235);
    private readonly Inventor.Application _app;
    private readonly ComboBox _presets = new();
    private readonly NumericUpDown _width = new();
    private readonly NumericUpDown _height = new();
    private readonly RadioButton _beside = new();
    private readonly RadioButton _fixed = new();
    private readonly System.Windows.Forms.TextBox _folder = new();
    private readonly Button _browse = new();
    private readonly Button _capture = new();
    private readonly Button _openFolder = new();
    private readonly Button _previewButton = new();
    private readonly PreviewCanvas _previewCanvas = new();
    private readonly Label _previewInfo = new();
    private readonly Label _status = new();
    private bool _isBusy;
    private bool _syncingPresets;
    private string? _lastSaved;

    public ScreenshotDialog(Inventor.Application app)
    {
        _app = app;
        Text = "Autodesk Inventor Better Screenshot Tool - By CalumK";
        ClientSize = new Size(1050, 696);
        MinimumSize = new Size(940, 700);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = BackgroundColor;
        ForeColor = Ink;

        try
        {
            using Stream? stream = typeof(ScreenshotDialog).Assembly.GetManifestResourceStream(
                "InventorScreenshotTool.Assets.BetterScreenshot.ico");
            if (stream is not null) Icon = new Icon(stream);
        }
        catch { /* The icon is decorative; it must not prevent capture. */ }

        // An explicit three-row layout prevents the header from hiding controls.
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = BackgroundColor
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 31, 55), Margin = Padding.Empty };
        root.Controls.Add(header, 0, 0);
        var logo = new PictureBox
        {
            Image = RibbonIcons.Load(48),
            SizeMode = PictureBoxSizeMode.Zoom
        };
        logo.SetBounds(25, 17, 50, 50);
        header.Controls.Add(logo);
        var title = MakeLabel("Better Screenshot Tool", 20.5f, Color.White, true);
        title.SetBounds(94, 12, 620, 40);
        header.Controls.Add(title);
        var subtitle = MakeLabel("For Autodesk Inventor 2025+    •    By CalumK", 9.5f,
            Color.FromArgb(194, 205, 226));
        subtitle.SetBounds(96, 52, 600, 23);
        header.Controls.Add(subtitle);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = BackgroundColor,
            Padding = new Padding(22, 18, 22, 12),
            Margin = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 445));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(content, 0, 1);

        var settingsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 14, 0),
            AutoScroll = true
        };
        content.Controls.Add(settingsPanel, 0, 0);

        var settings = SettingsStore.Load();
        AddLabel(settingsPanel, "EXPORT SETTINGS", 12f, Ink, true, 22, 21, 370, 28);
        AddLabel(settingsPanel, "Capture the active part or assembly as a white PNG.", 9f,
            Muted, false, 22, 53, 385, 28);
        AddLabel(settingsPanel, "Resolution", 10f, Ink, true, 22, 105, 200, 26);

        _presets.DropDownStyle = ComboBoxStyle.DropDownList;
        _presets.SetBounds(22, 144, 152, 30);
        _presets.Items.AddRange(new object[] { "1920 × 1080", "2048 × 2048", "3840 × 2160", "Custom" });
        settingsPanel.Controls.Add(_presets);
        AddLabel(settingsPanel, "W", 9f, Muted, false, 185, 146, 22, 25);
        _width.SetBounds(206, 143, 91, 29);
        SetupPixels(_width, settings.Width);
        settingsPanel.Controls.Add(_width);
        AddLabel(settingsPanel, "H", 9f, Muted, false, 308, 146, 22, 25);
        _height.SetBounds(327, 143, 90, 29);
        SetupPixels(_height, settings.Height);
        settingsPanel.Controls.Add(_height);
        _presets.SelectedIndexChanged += PresetChanged;
        _width.ValueChanged += DimensionsChanged;
        _height.ValueChanged += DimensionsChanged;
        UpdateSelectedPreset();

        AddLabel(settingsPanel, "Save location", 10f, Ink, true, 22, 206, 260, 26);
        _beside.Text = "Screenshots folder beside active model";
        _beside.SetBounds(22, 243, 391, 28);
        _beside.CheckedChanged += LocationModeChanged;
        settingsPanel.Controls.Add(_beside);
        _fixed.Text = "Use a fixed folder";
        _fixed.SetBounds(22, 286, 391, 28);
        _fixed.CheckedChanged += LocationModeChanged;
        settingsPanel.Controls.Add(_fixed);
        _folder.SetBounds(41, 330, 275, 29);
        _folder.Text = settings.FixedFolder;
        settingsPanel.Controls.Add(_folder);
        _browse.Text = "Browse";
        _browse.SetBounds(324, 329, 93, 30);
        _browse.Click += (_, _) => ChooseFolder();
        settingsPanel.Controls.Add(_browse);
        _beside.Checked = settings.LocationMode == SaveLocationMode.BesideDocument;
        _fixed.Checked = !_beside.Checked;
        LocationModeChanged(this, EventArgs.Empty);

        AddLabel(settingsPanel,
            "A saved .ipt or .iam is required. Numbered screenshots are never overwritten.",
            9f, Muted, false, 22, 380, 389, 42);

        _capture.Text = "Save PNG";
        _capture.BackColor = Accent;
        _capture.ForeColor = Color.White;
        _capture.FlatStyle = FlatStyle.Flat;
        _capture.FlatAppearance.BorderSize = 0;
        _capture.Font = new Font("Segoe UI Semibold", 10f);
        _capture.SetBounds(22, 450, 161, 42);
        _capture.Click += (_, _) => CaptureScreenshot();
        settingsPanel.Controls.Add(_capture);
        AcceptButton = _capture;

        _openFolder.Text = "Open folder";
        _openFolder.SetBounds(196, 450, 125, 42);
        _openFolder.Enabled = false;
        _openFolder.Click += (_, _) => OpenLastFolder();
        settingsPanel.Controls.Add(_openFolder);
        var close = new Button { Text = "Close" };
        close.SetBounds(332, 450, 85, 42);
        close.Click += (_, _) => Close();
        settingsPanel.Controls.Add(close);
        CancelButton = close;

        // No white preview card: the paper is drawn centrally on a grey canvas.
        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = BackgroundColor,
            Margin = Padding.Empty,
            Padding = new Padding(12, 0, 0, 0)
        };
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
        content.Controls.Add(right, 1, 0);

        var previewToolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColor,
            Margin = Padding.Empty,
            ColumnCount = 2,
            RowCount = 1
        };
        previewToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        previewToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 143));
        previewToolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.Controls.Add(previewToolbar, 0, 0);
        var previewTitle = MakeLabel("PREVIEW", 12f, Ink, true);
        previewTitle.Dock = DockStyle.Fill;
        previewTitle.Margin = Padding.Empty;
        previewToolbar.Controls.Add(previewTitle, 0, 0);
        _previewButton.Text = "Refresh preview";
        _previewButton.Dock = DockStyle.Top;
        _previewButton.Height = 32;
        _previewButton.Margin = Padding.Empty;
        previewToolbar.Controls.Add(_previewButton, 1, 0);
        _previewButton.Click += (_, _) => RefreshPreview(true);

        _previewCanvas.Dock = DockStyle.Fill;
        _previewCanvas.Margin = Padding.Empty;
        right.Controls.Add(_previewCanvas, 0, 1);
        _previewInfo.Text = "Reduced-resolution preview. Save PNG exports at full resolution.";
        _previewInfo.ForeColor = Muted;
        _previewInfo.TextAlign = ContentAlignment.MiddleLeft;
        _previewInfo.Dock = DockStyle.Fill;
        _previewInfo.Margin = Padding.Empty;
        _previewInfo.AutoEllipsis = true;
        right.Controls.Add(_previewInfo, 0, 2);

        var footer = new Panel { Dock = DockStyle.Fill, BackColor = BackgroundColor, Margin = Padding.Empty };
        root.Controls.Add(footer, 0, 2);
        _status.Text = "Ready.";
        _status.AutoEllipsis = true;
        _status.ForeColor = Muted;
        _status.SetBounds(26, 7, 960, 27);
        _status.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        footer.Controls.Add(_status);
        var credit = new LinkLabel
        {
            Text = "calumk.com",
            AutoSize = true,
            LinkColor = Accent,
            ActiveLinkColor = Color.FromArgb(30, 64, 175),
            Location = new Point(26, 42)
        };
        credit.LinkClicked += (_, _) =>
        {
            try { Process.Start(new ProcessStartInfo("https://calumk.com") { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Open website", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        footer.Controls.Add(credit);
        var version = typeof(ScreenshotDialog).Assembly.GetName().Version;
        var byline = MakeLabel($"Better Screenshot Tool  •  v{version?.ToString(3) ?? "0.3.1"}", 9f, Muted);
        byline.TextAlign = ContentAlignment.MiddleRight;
        byline.Size = new Size(265, 25);
        byline.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        footer.Controls.Add(byline);
        footer.Resize += (_, _) =>
        {
            byline.Location = new Point(footer.ClientSize.Width - byline.Width - 26, 40);
            _status.Width = Math.Max(100, footer.ClientSize.Width - 52);
        };

        // Defer COM rendering until the root has received its real initial size.
        Shown += (_, _) => BeginInvoke(new Action(() => RefreshPreview(false)));
    }

    private static Label MakeLabel(string text, float size, Color color, bool bold = false) => new()
    {
        Text = text, ForeColor = color, Font = new Font("Segoe UI", size,
            bold ? FontStyle.Bold : FontStyle.Regular), AutoSize = false,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static void AddLabel(Control parent, string text, float size, Color color, bool bold,
        int x, int y, int width, int height)
    {
        var label = MakeLabel(text, size, color, bold);
        label.SetBounds(x, y, width, height);
        parent.Controls.Add(label);
    }

    private static void SetupPixels(NumericUpDown field, int value)
    {
        field.Minimum = 256;
        field.Maximum = 8192;
        field.Increment = 64;
        field.Value = Math.Clamp(value, 256, 8192);
        field.ThousandsSeparator = true;
    }

    private void PresetChanged(object? sender, EventArgs e)
    {
        if (_syncingPresets) return;
        (int w, int h) = _presets.SelectedIndex switch
        {
            0 => (1920, 1080),
            1 => (2048, 2048),
            2 => (3840, 2160),
            _ => ((int)_width.Value, (int)_height.Value)
        };
        _syncingPresets = true;
        try { _width.Value = w; _height.Value = h; }
        finally { _syncingPresets = false; }
        DimensionsChanged(this, EventArgs.Empty);
    }

    private void DimensionsChanged(object? sender, EventArgs e)
    {
        if (_syncingPresets) return;
        UpdateSelectedPreset();
        if (_previewCanvas.HasImage)
            _previewInfo.Text = "Resolution changed — click Refresh preview to update.";
    }

    private void UpdateSelectedPreset()
    {
        int index = ((int)_width.Value, (int)_height.Value) switch
        {
            (1920, 1080) => 0,
            (2048, 2048) => 1,
            (3840, 2160) => 2,
            _ => 3
        };
        if (_presets.SelectedIndex == index) return;
        _syncingPresets = true;
        try { _presets.SelectedIndex = index; }
        finally { _syncingPresets = false; }
    }

    private void LocationModeChanged(object? sender, EventArgs e)
    {
        _folder.Enabled = _fixed.Checked;
        _browse.Enabled = _fixed.Checked;
    }

    private void ChooseFolder()
    {
        using var chooser = new FolderBrowserDialog
        {
            Description = "Choose a folder for screenshot PNGs",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(_folder.Text) ? _folder.Text : string.Empty
        };
        if (chooser.ShowDialog(this) == DialogResult.OK) _folder.Text = chooser.SelectedPath;
    }

    private ScreenshotOptions CurrentOptions() => new()
    {
        Width = (int)_width.Value,
        Height = (int)_height.Value,
        LocationMode = _fixed.Checked ? SaveLocationMode.FixedFolder : SaveLocationMode.BesideDocument,
        FixedFolder = _folder.Text.Trim()
    };

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        _capture.Enabled = !busy;
        _previewButton.Enabled = !busy;
        _openFolder.Enabled = !busy && _lastSaved is not null;
        UseWaitCursor = busy;
    }

    private void ShowImage(Bitmap bitmap, string description)
    {
        _previewCanvas.ReplaceImage(bitmap); // canvas assumes ownership; disposes prior image
        _previewInfo.Text = description;
    }

    private void RefreshPreview(bool showErrors)
    {
        if (_isBusy) return;
        SetBusy(true);
        _status.Text = "Rendering preview…";
        try
        {
            Bitmap result = ScreenshotExporter.CreatePreview(_app, CurrentOptions());
            ShowImage(result, $"Preview • {_width.Value:0} × {_height.Value:0} output • white background");
            _status.Text = "Preview ready. Save PNG when satisfied.";
        }
        catch (Exception ex)
        {
            if (!_previewCanvas.HasImage)
                _previewInfo.Text = "Preview unavailable. Save the active model first.";
            _status.Text = "Preview unavailable: " + ex.Message;
            if (showErrors)
                MessageBox.Show(this, ex.Message, "Preview unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally { SetBusy(false); }
    }

    private void CaptureScreenshot()
    {
        if (_isBusy) return;
        SetBusy(true);
        try
        {
            ScreenshotOptions options = CurrentOptions();
            string file = ScreenshotExporter.Capture(_app, options);
            _lastSaved = file;
            _status.Text = "Saved: " + file;
            _app.StatusBarText = "Screenshot saved: " + file;
            try
            {
                ShowImage(ScreenshotExporter.LoadThumbnail(file, 960, 620),
                    "Saved • " + System.IO.Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                _previewInfo.Text = "PNG saved. Could not show preview: " + ex.Message;
            }
            try { SettingsStore.Save(options); }
            catch (Exception ex) { _status.Text = "PNG saved, but preferences could not be saved: " + ex.Message; }
        }
        catch (Exception ex)
        {
            _status.Text = "Capture failed: " + ex.Message;
            MessageBox.Show(this, ex.Message, "Better Screenshot Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { SetBusy(false); }
    }

    private void OpenLastFolder()
    {
        if (_lastSaved is null) return;
        string? folder = System.IO.Path.GetDirectoryName(_lastSaved);
        if (folder is not null && Directory.Exists(folder))
            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
    }
}
