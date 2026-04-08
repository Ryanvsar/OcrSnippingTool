using OcrSnippingTool.Interop;

namespace OcrSnippingTool.Models;

public class AppSettings
{
    /// <summary>Bitmask of modifier keys (MOD_ALT=1, MOD_CONTROL=2, MOD_SHIFT=4, MOD_WIN=8).</summary>
    public uint HotkeyModifiers { get; set; } = NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT;

    /// <summary>Virtual key code for the hotkey trigger. Default 0x58 = 'X'.</summary>
    public uint HotkeyVirtualKey { get; set; } = 0x58;

    /// <summary>BCP-47 language tag used for OCR recognition.</summary>
    public string OcrLanguage { get; set; } = "en-US";

    /// <summary>Whether to launch with Windows (registry / StartupTask).</summary>
    public bool AutoStart { get; set; } = false;

    /// <summary>Show a tray balloon notification after each copy.</summary>
    public bool ShowNotificationOnCopy { get; set; } = true;
}
