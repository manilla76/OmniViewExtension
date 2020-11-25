using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using OpcClientUI.Services;
using System;
using System.Collections.Generic;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace OpcClientUI.ViewModels
{
    public class WindowTwoViewModel : ViewModelBase, IActivable, IHidable
    {
        private readonly NavigationService navigationService;
        private string data;
        public string Data { get => data; set => Set(ref data, value); }

        public RelayCommand BackCommand { get; }
        private bool isVisible;
        public bool IsVisible { get => isVisible; set => Set(ref isVisible, value); }

        public WindowTwoViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
            BackCommand = new RelayCommand(async () => await BackAsync());
        }

        private Task BackAsync()
        {
            ChangeVisiblity(false);
            return navigationService.ShowAsync(Windows.MainWindow);
        }

        public Task ActivateAsync(object parameter)
        {
            Data = parameter?.ToString();
            return Task.CompletedTask;
        }

        public void ChangeVisiblity(bool isVisible)
        {
            throw new NotImplementedException();
        }
    }
}
