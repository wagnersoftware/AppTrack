using System.Windows;
using System.Windows.Controls;

namespace AppTrack.WpfUi.Helpers;

/// <summary>
/// Provides a dependency property for the bound password, to set the password from the viewmodel -> view. (PasswordBox doesn't supprt bindings)
/// </summary>
public static class PasswordBoxHelper
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static string GetBoundPassword(DependencyObject d)
        => (string)d.GetValue(BoundPasswordProperty);

    public static void SetBoundPassword(DependencyObject d, string value)
        => d.SetValue(BoundPasswordProperty, value);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            passwordBox.Password = e.NewValue as string ?? string.Empty;
        }
    }
}

