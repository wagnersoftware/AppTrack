using System.Windows;

namespace AppTrack.WpfUi.MessageBoxService;

public class MessageBoxService : IMessageBoxService
{
    public MessageBoxResult ShowErrorMessageBox(string message, string caption = "Error")
    {
        var owner = GetActiveWindow();
        return MessageBox.Show(owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public MessageBoxResult ShowQuestionMessageBox(string message, string caption)
    {
        var owner = GetActiveWindow();
        return MessageBox.Show(owner, message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question);
    }

    private Window GetActiveWindow() =>
        Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.IsActive)
        ?? Application.Current.MainWindow;
}
