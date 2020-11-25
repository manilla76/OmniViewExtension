using GalaSoft.MvvmLight;
using OpcClientUI.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpcClientUI.ViewModels
{
    public class DetailViewModel : ViewModelBase, IActivable
    {
        private string parameter;
        public string Parameter
        {
            get => parameter;
            set => Set(ref parameter, value);
        }


        public Task ActivateAsync(object parameter)
        {
            Parameter = parameter?.ToString();
            return Task.CompletedTask;
        }
    }
}
