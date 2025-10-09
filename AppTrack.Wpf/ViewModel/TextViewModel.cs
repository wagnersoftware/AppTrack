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

        /// <summary>
        /// Copy text for text editors with \n line endings.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanCopy))]
        public async Task CopyTextPlain()
        {
            try
            {
                Clipboard.SetText(Text);

                await ShowUserMessageAsync("Text copied.");
            }
            catch
            {
                await ShowUserMessageAsync("Error: Could not copy text");
            }
        }

        private bool CanCopy()
        {
            return !string.IsNullOrWhiteSpace(Text);
        }

        private async Task ShowUserMessageAsync(string message, int durationMs = 3000)
        {
            UserMessage = message;
            await Task.Delay(durationMs);
            UserMessage = string.Empty;
        }
    }
}
