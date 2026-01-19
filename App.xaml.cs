using System.Threading.Tasks;
using System.Windows;

namespace Watson
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await WindowsHandler.InitializeAsync();
        }
    }
}
