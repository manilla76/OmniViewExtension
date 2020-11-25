using Microsoft.Extensions.DependencyInjection;
using OpcClientUI.ViewModels;


namespace OpcClientUI.ViewModels
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel => App.ServiceProvider.GetRequiredService<MainViewModel>();
        public DetailViewModel DetailViewModel => App.ServiceProvider.GetRequiredService<DetailViewModel>();
        public WindowTwoViewModel WindowTwoViewModel => App.ServiceProvider.GetRequiredService<WindowTwoViewModel>();
    }
}
