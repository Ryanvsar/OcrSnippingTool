using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using OcrSnippingTool.Interop;

namespace OcrSnippingTool.Services;

public static class ScreenshotService
{
    /// <summary>
    /// Captures a rectangle of the virtual screen in physical pixels.
    /// physicalRect coordinates are relative to the virtual screen origin.
    /// </summary>
    public static BitmapSource CaptureRegion(Int32Rect physicalRect)
    {
        int w = physicalRect.Width;
        int h = physicalRect.Height;

        IntPtr hdcScreen = NativeMethods.GetDC(IntPtr.Zero);
        IntPtr hdcMem    = NativeMethods.CreateCompatibleDC(hdcScreen);
        IntPtr hBmp      = NativeMethods.CreateCompatibleBitmap(hdcScreen, w, h);
        IntPtr hOld      = NativeMethods.SelectObject(hdcMem, hBmp);

        NativeMethods.BitBlt(hdcMem, 0, 0, w, h,
            hdcScreen, physicalRect.X, physicalRect.Y,
            NativeMethods.SRCCOPY);

        NativeMethods.SelectObject(hdcMem, hOld);
        NativeMethods.DeleteDC(hdcMem);
        NativeMethods.ReleaseDC(IntPtr.Zero, hdcScreen);

        // Convert HBITMAP → WPF BitmapSource
        var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
            hBmp,
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        bitmapSource.Freeze(); // allow cross-thread access

        NativeMethods.DeleteObject(hBmp);
        return bitmapSource;
    }
}
