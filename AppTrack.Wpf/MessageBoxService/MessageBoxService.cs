using System.Windows;

namespace AppTrack.WpfUi.MessageBoxService;

public class MessageBoxService : IMessageBoxService
{
    public MessageBoxResult ShowErrorMessageBox(string message, string caption = "Error")
    {
        return MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public MessageBoxResult ShowQuestionMessageBox(string message, string caption)
    {
        return MessageBox.Show(message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question);
    }
}
