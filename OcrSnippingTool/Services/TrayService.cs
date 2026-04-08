using System.Drawing;
using System.Windows.Forms;
using OcrSnippingTool.Interop;
using OcrSnippingTool.Models;

namespace OcrSnippingTool.Services;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly SettingsService _settings;

    public event Action? OpenSettingsRequested;
    public event Action? ExitRequested;

    public TrayService(SettingsService settings)
    {
        _settings = settings;

        _notifyIcon = new NotifyIcon
        {
            Icon    = CreateIcon(),
            Text    = "OCR Snipping Tool  (Ctrl+Shift+X to snip)",
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => OpenSettingsRequested?.Invoke();
        BuildContextMenu();
    }

    private void BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        var snipItem = new ToolStripMenuItem("Snip Now  (Ctrl+Shift+X)");
        snipItem.Font = new Font(snipItem.Font, FontStyle.Bold);
        snipItem.Click += (_, _) => SnipRequested?.Invoke();

        menu.Items.Add(snipItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Settings", null, (_, _) => OpenSettingsRequested?.Invoke());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitRequested?.Invoke());

        _notifyIcon.ContextMenuStrip = menu;
    }

    public event Action? SnipRequested;

    public void ShowBalloonTip(string title, string text)
    {
        if (!_settings.Current.ShowNotificationOnCopy) return;

        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText  = text;
        _notifyIcon.BalloonTipIcon  = ToolTipIcon.Info;
        _notifyIcon.ShowBalloonTip(2500);
    }

    public void UpdateTooltip(string text) => _notifyIcon.Text = text;

    private static Icon CreateIcon()
    {
        // Create a simple 32x32 icon programmatically — no external file needed
        using var bmp = new Bitmap(32, 32);
        using var g   = Graphics.FromImage(bmp);

        g.Clear(Color.Transparent);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Blue rounded square background
        using var bgBrush = new SolidBrush(Color.FromArgb(255, 30, 120, 210));
        g.FillRectangle(bgBrush, 2, 2, 28, 28);

        // White scissors-like cross
        using var pen = new Pen(Color.White, 3f);
        g.DrawLine(pen, 8, 12, 24, 12);
        g.DrawLine(pen, 8, 20, 24, 20);
        g.DrawLine(pen, 12, 6, 12, 26);

        var iconHandle = bmp.GetHicon();
        var icon = (Icon)Icon.FromHandle(iconHandle).Clone();
        NativeMethods.DestroyIcon(iconHandle);
        return icon;
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
