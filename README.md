# OCR Snipping Tool
A lightweight, tray-based Windows utility that lets you draw a selection box over any part of your screen and instantly copy the text to your clipboard — even when the text isn't normally selectable (images, videos, PDFs, games, etc.).

Everything runs locally using Windows' built-in OCR engine. No internet connection, no third-party APIs.

# Features
- Global hotkey — press Ctrl+Shift+X from anywhere to activate
- Draw to select — drag a rectangle over any screen region
- Instant clipboard copy — text is copied automatically on release
- Multi-monitor support — works across all displays
- System tray — runs quietly in the background, no taskbar clutter
- Configurable — change hotkey, OCR language, and auto-start from the settings window
- No external dependencies — uses Windows.Media.Ocr built into Windows 10/11

# Requirements
Windows 10 (build 19041+) or Windows 11
.NET 10 SDK

# Getting Started
git clone https://github.com/Ryanvsar/OcrSnippingTool.git
cd OcrSnippingTool
dotnet run --project OcrSnippingTool

The app starts in the system tray. Press Ctrl+Shift+X to snip.

# Usage
Action	How
Activate snip	Ctrl+Shift+X
Select region	Click and drag
Cancel	Esc
Open settings	Right-click tray icon → Settings
Snip via tray	Right-click tray icon → Snip Now

# Settings
Right-click the tray icon and choose Settings to configure:
- Hotkey — record any key combination
- OCR Language — choose from languages installed on your system
- Start with Windows — launch automatically on login
- Notifications — toggle the copy confirmation balloon

# Built With
WPF / .NET 10
Windows.Media.Ocr (built-in Windows OCR)
Win32 P/Invoke (RegisterHotKey, BitBlt)
