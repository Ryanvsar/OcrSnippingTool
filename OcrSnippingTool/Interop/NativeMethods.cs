using System.Runtime.InteropServices;

namespace OcrSnippingTool.Interop;

internal static class NativeMethods
{
    // ── Hotkey ────────────────────────────────────────────────────────────────
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    internal const int    WM_HOTKEY    = 0x0312;
    internal const uint   MOD_ALT      = 0x0001;
    internal const uint   MOD_CONTROL  = 0x0002;
    internal const uint   MOD_SHIFT    = 0x0004;
    internal const uint   MOD_WIN      = 0x0008;
    internal const uint   MOD_NOREPEAT = 0x4000;

    // ── Screenshot (BitBlt) ───────────────────────────────────────────────────
    [DllImport("gdi32.dll")]
    internal static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest,
        int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, uint dwRop);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

    [DllImport("gdi32.dll")]
    internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport("gdi32.dll")]
    internal static extern bool DeleteObject(IntPtr ho);

    [DllImport("gdi32.dll")]
    internal static extern bool DeleteDC(IntPtr hdc);

    internal const uint SRCCOPY = 0x00CC0020;

    // ── Message-only window parent ────────────────────────────────────────────
    internal static readonly IntPtr HWND_MESSAGE = new(-3);

    // ── Icon management ───────────────────────────────────────────────────────
    [DllImport("user32.dll")]
    internal static extern bool DestroyIcon(IntPtr hIcon);
}
