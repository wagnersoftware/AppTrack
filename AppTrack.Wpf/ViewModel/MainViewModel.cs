using AppTrack.Frontend.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AppTrack.WpfUi.ViewModel
{
    public class MainViewModel: ObservableObject
    {
        public ObservableCollection<JobApplicationModel> JobApplication { get; set; }
    }
}
