using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Extensions.Options;
using OpcClientUI.Models;
using OpcClientUI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpcClientUI.ViewModels
{
    public class MainViewModel : ViewModelBase, IHidable
    {
        private readonly NavigationService navigationService;
        private readonly ISampleService sampleService;
        private readonly AppSettings settings;

        private string input;
        public string Input
        {
            get { return input; }
            set { Set(ref input, value); }
        }
        
        public string Time
        {
            get => sampleService.GetCurrentDate();             
        }

        public RelayCommand ExecuteCommand { get; }
        public RelayCommand NextCommand { get; }
        private bool isVisible;
        public bool IsVisible { get => isVisible; set => Set(ref isVisible, value); }

        public MainViewModel(NavigationService navigationService, ISampleService sampleService, IOptions<AppSettings> options)
        {
            this.navigationService = navigationService;
            this.sampleService = sampleService;
            settings = options.Value;

            ExecuteCommand = new RelayCommand(async () => await ExecuteAsync());
            NextCommand = new RelayCommand(async () => await NextAsync());
        }

        private Task NextAsync()
        {
            ChangeVisiblity(false);
            return navigationService.ShowAsync(Windows.WindowTwo, "This is Window Two");
        }

        private Task ExecuteAsync()
        {
            Debug.WriteLine($"Current value: {input}");
            return navigationService.ShowDialogAsync(Windows.DetailWindow, input);
        }

        public void ChangeVisiblity(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }
}
