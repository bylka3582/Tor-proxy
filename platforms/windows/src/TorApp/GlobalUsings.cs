// UseWindowsForms pulls System.Windows.Forms into the implicit global usings,
// which collides with WPF on a few common names. This is a WPF app, so alias the
// ambiguous names to their WPF meaning; WinForms types are referenced explicitly
// (e.g. via the `WinForms` alias in the tray code).
global using Application = System.Windows.Application;
global using MessageBox = System.Windows.MessageBox;
global using Clipboard = System.Windows.Clipboard;
