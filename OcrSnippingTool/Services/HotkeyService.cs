using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using OcrSnippingTool.Interop;
using Application = System.Windows.Application;
using OcrSnippingTool.Models;

namespace OcrSnippingTool.Services;

public sealed class HotkeyService : IDisposable
{
    private const int HotkeyId = 9001;

    private HwndSource? _hwndSource;
    private readonly SettingsService _settings;

    public event Action? HotkeyTriggered;

    public HotkeyService(SettingsService settings) => _settings = settings;

    public void Register()
    {
        // Message-only hidden window — no taskbar/Alt+Tab presence
        var param = new HwndSourceParameters("OcrSnipHotkeyWindow")
        {
            HwndSourceHook = WndProc,
            ParentWindow   = NativeMethods.HWND_MESSAGE,
            Width          = 0,
            Height         = 0,
            WindowStyle    = 0
        };
        _hwndSource = new HwndSource(param);

        var s = _settings.Current;
        bool ok = NativeMethods.RegisterHotKey(
            _hwndSource.Handle,
            HotkeyId,
            s.HotkeyModifiers | NativeMethods.MOD_NOREPEAT,
            s.HotkeyVirtualKey);

        if (!ok)
        {
            int err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err,
                $"Could not register hotkey (error {err}). The key combination may be in use by another application.");
        }
    }

    public void Reregister()
    {
        if (_hwndSource != null)
            NativeMethods.UnregisterHotKey(_hwndSource.Handle, HotkeyId);

        // Re-read settings and re-register
        var s = _settings.Current;
        bool ok = NativeMethods.RegisterHotKey(
            _hwndSource!.Handle,
            HotkeyId,
            s.HotkeyModifiers | NativeMethods.MOD_NOREPEAT,
            s.HotkeyVirtualKey);

        if (!ok)
        {
            int err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err,
                $"Could not register new hotkey (error {err}). The key combination may be in use.");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            Application.Current?.Dispatcher.Invoke(() => HotkeyTriggered?.Invoke());
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_hwndSource != null)
        {
            NativeMethods.UnregisterHotKey(_hwndSource.Handle, HotkeyId);
            _hwndSource.Dispose();
            _hwndSource = null;
        }
    }
}
