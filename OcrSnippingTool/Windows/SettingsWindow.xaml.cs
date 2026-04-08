using System.Text;
using System.Windows;
using System.Windows.Input;
using OcrSnippingTool.Interop;
using OcrSnippingTool.Models;
using OcrSnippingTool.Services;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace OcrSnippingTool.Windows;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly HotkeyService   _hotkeyService;

    // Working copy — only committed on Save
    private uint _editModifiers;
    private uint _editVirtualKey;

    private bool _recordingHotkey;

    public SettingsWindow(SettingsService settingsService, HotkeyService hotkeyService)
    {
        InitializeComponent();
        _settingsService = settingsService;
        _hotkeyService   = hotkeyService;

        var s = settingsService.Current;
        _editModifiers  = s.HotkeyModifiers;
        _editVirtualKey = s.HotkeyVirtualKey;

        HotkeyBox.Text    = FormatHotkey(_editModifiers, _editVirtualKey);
        AutoStartCheck.IsChecked = s.AutoStart;
        NotifyCheck.IsChecked    = s.ShowNotificationOnCopy;

        var langs = OcrService.GetAvailableLanguages();
        LanguageCombo.ItemsSource   = langs;
        LanguageCombo.SelectedItem  = langs.Contains(s.OcrLanguage) ? s.OcrLanguage : langs.FirstOrDefault();
    }

    // ── Hotkey recording ─────────────────────────────────────────────────────

    private void OnRecordClick(object sender, RoutedEventArgs e)
    {
        _recordingHotkey = true;
        HotkeyBox.Text   = "Press a key combination…";
        HotkeyBox.Focus();
        RecordBtn.IsEnabled = false;
    }

    private void OnHotkeyBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (!_recordingHotkey) return;

        e.Handled = true;

        // Ignore standalone modifier presses
        if (e.Key is Key.LeftShift or Key.RightShift
                  or Key.LeftCtrl  or Key.RightCtrl
                  or Key.LeftAlt   or Key.RightAlt
                  or Key.LWin      or Key.RWin
                  or Key.System)
            return;

        if (e.Key == Key.Escape)
        {
            CancelRecording();
            return;
        }

        var mods = e.KeyboardDevice.Modifiers;
        uint modFlags = 0;
        if ((mods & ModifierKeys.Alt)     != 0) modFlags |= NativeMethods.MOD_ALT;
        if ((mods & ModifierKeys.Control) != 0) modFlags |= NativeMethods.MOD_CONTROL;
        if ((mods & ModifierKeys.Shift)   != 0) modFlags |= NativeMethods.MOD_SHIFT;
        if ((mods & ModifierKeys.Windows) != 0) modFlags |= NativeMethods.MOD_WIN;

        // Require at least one modifier
        if (modFlags == 0)
        {
            HotkeyBox.Text = "Include at least one modifier (Ctrl, Shift, Alt, Win)";
            return;
        }

        uint vk = (uint)KeyInterop.VirtualKeyFromKey(e.Key);
        _editModifiers  = modFlags;
        _editVirtualKey = vk;

        _recordingHotkey    = false;
        RecordBtn.IsEnabled = true;
        HotkeyBox.Text = FormatHotkey(_editModifiers, _editVirtualKey);
    }

    private void CancelRecording()
    {
        _recordingHotkey    = false;
        RecordBtn.IsEnabled = true;
        HotkeyBox.Text = FormatHotkey(_editModifiers, _editVirtualKey);
    }

    private void OnResetHotkey(object sender, RoutedEventArgs e)
    {
        var defaults    = new AppSettings();
        _editModifiers  = defaults.HotkeyModifiers;
        _editVirtualKey = defaults.HotkeyVirtualKey;
        HotkeyBox.Text  = FormatHotkey(_editModifiers, _editVirtualKey);
        _recordingHotkey    = false;
        RecordBtn.IsEnabled = true;
    }

    // ── Save / Cancel ────────────────────────────────────────────────────────

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var s = _settingsService.Current;
        s.HotkeyModifiers  = _editModifiers;
        s.HotkeyVirtualKey = _editVirtualKey;
        s.OcrLanguage      = LanguageCombo.SelectedItem as string ?? s.OcrLanguage;
        s.AutoStart        = AutoStartCheck.IsChecked == true;
        s.ShowNotificationOnCopy = NotifyCheck.IsChecked == true;

        _settingsService.Save();

        try
        {
            _hotkeyService.Reregister();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Settings saved, but could not register new hotkey:\n{ex.Message}",
                "Hotkey Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string FormatHotkey(uint modifiers, uint vk)
    {
        var sb = new StringBuilder();
        if ((modifiers & NativeMethods.MOD_WIN)     != 0) sb.Append("Win+");
        if ((modifiers & NativeMethods.MOD_CONTROL) != 0) sb.Append("Ctrl+");
        if ((modifiers & NativeMethods.MOD_ALT)     != 0) sb.Append("Alt+");
        if ((modifiers & NativeMethods.MOD_SHIFT)   != 0) sb.Append("Shift+");

        var key = KeyInterop.KeyFromVirtualKey((int)vk);
        sb.Append(key == Key.None ? $"VK{vk:X2}" : key.ToString());
        return sb.ToString();
    }
}
