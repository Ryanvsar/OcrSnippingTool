using System.IO;
using System.Windows.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using WpfBitmapFrame = System.Windows.Media.Imaging.BitmapFrame;
using WpfBitmapDecoder = System.Windows.Media.Imaging.BitmapDecoder;
using WinBitmapDecoder = Windows.Graphics.Imaging.BitmapDecoder;

namespace OcrSnippingTool.Services;

public class OcrService
{
    private readonly SettingsService _settings;

    public OcrService(SettingsService settings) => _settings = settings;

    public async Task<string> RecognizeAsync(BitmapSource bitmapSource)
    {
        var engine = CreateEngine(_settings.Current.OcrLanguage);

        var softwareBitmap = await ConvertToSoftwareBitmapAsync(bitmapSource);
        var result         = await engine.RecognizeAsync(softwareBitmap);

        return string.Join(Environment.NewLine,
            result.Lines.Select(l => l.Text));
    }

    private static OcrEngine CreateEngine(string languageTag)
    {
        // Try the configured language first, fall back to user profile languages
        var lang = new global::Windows.Globalization.Language(languageTag);
        if (OcrEngine.IsLanguageSupported(lang))
        {
            var engine = OcrEngine.TryCreateFromLanguage(lang);
            if (engine != null) return engine;
        }

        return OcrEngine.TryCreateFromUserProfileLanguages()
               ?? throw new InvalidOperationException(
                   "No supported OCR language found. Please install an OCR language pack in Windows Settings.");
    }

    private static async Task<SoftwareBitmap> ConvertToSoftwareBitmapAsync(BitmapSource src)
    {
        // Encode WPF BitmapSource to PNG bytes
        var pngEncoder = new PngBitmapEncoder();
        pngEncoder.Frames.Add(WpfBitmapFrame.Create(src));
        using var dotNetStream = new MemoryStream();
        pngEncoder.Save(dotNetStream);
        byte[] pngBytes = dotNetStream.ToArray();

        // Write into a WinRT InMemoryRandomAccessStream via DataWriter
        var ras = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(ras.GetOutputStreamAt(0)))
        {
            writer.WriteBytes(pngBytes);
            await writer.StoreAsync();
            await writer.FlushAsync();
            writer.DetachStream(); // don't close ras when writer disposes
        }
        ras.Seek(0);

        var decoder = await WinBitmapDecoder.CreateAsync(ras);
        return await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied);
    }

    /// <summary>Returns language tags for all OCR languages installed on this machine.</summary>
    public static IReadOnlyList<string> GetAvailableLanguages() =>
        OcrEngine.AvailableRecognizerLanguages
                 .Select(l => l.LanguageTag)
                 .OrderBy(t => t)
                 .ToList();
}
