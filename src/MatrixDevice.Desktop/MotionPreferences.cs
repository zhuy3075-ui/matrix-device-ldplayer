using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace MatrixDevice.Desktop;

public sealed class MotionPreferences : INotifyPropertyChanged
{
    public static MotionPreferences Current { get; } = new();
    private bool? fixturePreference;
    public bool AnimationsEnabled => fixturePreference ?? SystemParameters.ClientAreaAnimation;
    public PopupAnimation PopupAnimation => AnimationsEnabled ? PopupAnimation.Fade : PopupAnimation.None;
    public event PropertyChangedEventHandler? PropertyChanged;

    private MotionPreferences()
    {
        SystemParameters.StaticPropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(SystemParameters.ClientAreaAnimation)) return;
            if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess()) dispatcher.BeginInvoke(Notify);
            else Notify();
        };
    }

    internal void SetFixturePreference(bool? enabled)
    {
        fixturePreference = enabled;
        Notify();
    }

    private void Notify()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AnimationsEnabled)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PopupAnimation)));
    }
}
