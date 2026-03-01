using AppTrack.Frontend.ApiService.Base;
using System.Windows;

namespace AppTrack.WpfUi.MessageBoxService;

public class MessageBoxService : IMessageBoxService
{
    /// <summary>
    /// Sets the response' validation error message as message box text if set, otherwise the response message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <returns></returns>
    public MessageBoxResult ShowErrorMessageBox<T>(Response<T> response)
    {
        var owner = GetActiveWindow();
        var message = string.IsNullOrEmpty(response.ValidationErrors) == false ? response.ValidationErrors : response.ErrorMessage;

        if (owner != null)
        {
            return MessageBox.Show(owner, message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        return MessageBox.Show(message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
    }


    public MessageBoxResult ShowErrorMessageBox(string message)
    {
        var owner = GetActiveWindow();
        if (owner != null)
        {
            return MessageBox.Show(owner, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public MessageBoxResult ShowInformationMessageBox(string message, string caption)
    {
        var owner = GetActiveWindow();
        if (owner != null)
        {
            return MessageBox.Show(owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        return MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public MessageBoxResult ShowQuestionMessageBox(string message, string caption)
    {
        var owner = GetActiveWindow();
        if (owner != null)
        {
            return MessageBox.Show(owner, message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question);
        }
        return MessageBox.Show(message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question);
    }

    private static Window GetActiveWindow() =>
        Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.IsActive)
        ?? Application.Current.MainWindow;
}
