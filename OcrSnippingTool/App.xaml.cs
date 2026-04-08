using System.Threading;
using System.Windows;
using OcrSnippingTool.Services;
using OcrSnippingTool.Windows;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace OcrSnippingTool;

public partial class App : Application
{
    private static Mutex? _mutex;

    private SettingsService? _settingsService;
    private OcrService?      _ocrService;
    private TrayService?     _trayService;
    private HotkeyService?   _hotkeyService;

    private bool _overlayOpen; // prevent stacking multiple overlays
    private SettingsWindow? _settingsWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Single-instance guard
        _mutex = new Mutex(true, "OcrSnippingTool_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "OCR Snipping Tool is already running.\nCheck the system tray.",
                "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        _settingsService = new SettingsService();
        _settingsService.Load();

        _ocrService  = new OcrService(_settingsService);
        _trayService = new TrayService(_settingsService);
        _trayService.OpenSettingsRequested += OpenSettings;
        _trayService.SnipRequested         += TriggerSnip;
        _trayService.ExitRequested         += () => Shutdown();

        _hotkeyService = new HotkeyService(_settingsService);
        _hotkeyService.HotkeyTriggered += TriggerSnip;

        try
        {
            _hotkeyService.Register();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not register hotkey:\n{ex.Message}\n\nYou can still snip via the tray icon.",
                "Hotkey Registration Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void TriggerSnip()
    {
        if (_overlayOpen) return; // don't stack overlays
        _overlayOpen = true;

        var overlay = new OverlayWindow();
        overlay.RegionSelected += OnRegionSelected;
        overlay.Closed         += (_, _) => _overlayOpen = false;
        overlay.Show();
    }

    private async void OnRegionSelected(System.Windows.Media.Imaging.BitmapSource screenshot)
    {
        try
        {
            string text = await _ocrService!.RecognizeAsync(screenshot);

            if (string.IsNullOrWhiteSpace(text))
            {
                _trayService!.ShowBalloonTip("No text found", "No text was detected in the selected region.");
                return;
            }

            Clipboard.SetText(text);
            _trayService!.ShowBalloonTip("Copied!", $"{text.Length} character{(text.Length == 1 ? "" : "s")} copied to clipboard.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OCR failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenSettings()
    {
        if (_settingsWindow != null && _settingsWindow.IsVisible)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_settingsService!, _hotkeyService!);
        _settingsWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyService?.Dispose();
        _trayService?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
