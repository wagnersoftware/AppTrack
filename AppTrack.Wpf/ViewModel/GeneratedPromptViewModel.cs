using AppTrack.Frontend.Models;
using AppTrack.WpfUi.Helpers;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel
{
    public partial class GeneratedPromptViewModel : TextViewModel
    {
        public GeneratedPromptViewModel(GeneratedPromptModel textModel) : base(textModel)
        {
            base.UserMessage = UserMessageHelper.GenerateUserMessageforUnusedKeys(textModel.UnusedKeys);
            base.WindowTitle = "Generated Prompt";
        }

        [RelayCommand(CanExecute = nameof(base.IsTextSet))]
        public void GenerateApplicationText(Window window)
        {
            window.DialogResult = true;
            window.Close();
        }
    }
}
