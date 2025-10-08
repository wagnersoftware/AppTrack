using AppTrack.Frontend.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel
{
    public partial class TextViewModel: ObservableObject
    {
        [ObservableProperty]
        private string windowTitle = "Text Window";

        [ObservableProperty]
        private string text = string.Empty;

        [ObservableProperty]
        private string userMessage = string.Empty;

        public TextViewModel(TextModel textModel)
        {
            WindowTitle = textModel.WindowTitle;
            Text = textModel.Text;
            UserMessage = textModel.UserMessage;
        }

        [RelayCommand]
        public void Close(Window window)
        {
            window.Close();
        }
    }
}
