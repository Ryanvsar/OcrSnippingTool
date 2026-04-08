using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OcrSnippingTool.Services;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Windows.Point;

namespace OcrSnippingTool.Windows;

public partial class OverlayWindow : Window
{
    private Point _startPoint;
    private bool  _isSelecting;

    /// <summary>Fires with the captured BitmapSource once the user releases the mouse.</summary>
    public event Action<BitmapSource>? RegionSelected;

    public OverlayWindow()
    {
        InitializeComponent();

        // Span the entire virtual desktop (all monitors)
        Left   = SystemParameters.VirtualScreenLeft;
        Top    = SystemParameters.VirtualScreenTop;
        Width  = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        _startPoint  = e.GetPosition(this);
        _isSelecting = true;

        HintBorder.Visibility = Visibility.Collapsed;
        SelectionRect.Visibility = Visibility.Visible;

        CaptureMouse(); // keep receiving events even if cursor drifts outside window
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isSelecting) return;
        var current = e.GetPosition(this);
        UpdateSelectionRect(_startPoint, current);
    }

    protected override async void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!_isSelecting) return;
        _isSelecting = false;
        ReleaseMouseCapture();

        var endPoint = e.GetPosition(this);
        var rect     = GetSelectionRect(_startPoint, endPoint);

        if (rect.Width < 5 || rect.Height < 5)
        {
            Close();
            return;
        }

        // Convert WPF DIPs → physical pixels
        var dpi = VisualTreeHelper.GetDpi(this);

        // Virtual screen origin in physical pixels
        double vsOriginX = SystemParameters.VirtualScreenLeft * dpi.DpiScaleX;
        double vsOriginY = SystemParameters.VirtualScreenTop  * dpi.DpiScaleY;

        var physicalRect = new Int32Rect(
            (int)(rect.X * dpi.DpiScaleX + vsOriginX),
            (int)(rect.Y * dpi.DpiScaleY + vsOriginY),
            (int)Math.Ceiling(rect.Width  * dpi.DpiScaleX),
            (int)Math.Ceiling(rect.Height * dpi.DpiScaleY));

        // Hide overlay before capture so it doesn't appear in the screenshot
        Hide();
        await Task.Delay(60); // let DWM flush the overlay from the screen compositor

        var screenshot = ScreenshotService.CaptureRegion(physicalRect);
        Close();
        RegionSelected?.Invoke(screenshot);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    private void UpdateSelectionRect(Point start, Point current)
    {
        var r = GetSelectionRect(start, current);
        Canvas.SetLeft(SelectionRect, r.X);
        Canvas.SetTop(SelectionRect,  r.Y);
        SelectionRect.Width  = r.Width;
        SelectionRect.Height = r.Height;

        // Update dimension label
        DimLabel.Text = $"{(int)r.Width} × {(int)r.Height}";
        Canvas.SetLeft(DimBorder, r.X + 4);
        Canvas.SetTop(DimBorder,  r.Y + r.Height + 4);
        DimBorder.Visibility = r.Width > 30 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static Rect GetSelectionRect(Point a, Point b) =>
        new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y),
            Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
}
