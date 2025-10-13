using AppTrack.Frontend.ApiService.Base;
using System.Windows;

namespace AppTrack.WpfUi.MessageBoxService;

public interface IMessageBoxService
{
    MessageBoxResult ShowErrorMessageBox<T>(Response<T> response);
    MessageBoxResult ShowErrorMessageBox(string message);

    MessageBoxResult ShowQuestionMessageBox(string message, string caption);

    MessageBoxResult ShowInformationMessageBox(string message, string caption);
}
