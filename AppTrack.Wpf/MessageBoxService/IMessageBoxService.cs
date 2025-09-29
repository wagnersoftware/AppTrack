using System.Windows;

namespace AppTrack.WpfUi.MessageBoxService;

public interface IMessageBoxService
{
    MessageBoxResult ShowErrorMessageBox(string message, string caption = null!);

    MessageBoxResult ShowQuestionMessageBox(string message, string caption);
}
