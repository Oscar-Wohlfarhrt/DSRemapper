using DSRemapper.RazorLayouts;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace DSRemapper
{
    public partial class LogConsole : Form
    {
        public LogConsole()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();
            blazorWebView1.HostPage = "wwwroot\\index.html";
            blazorWebView1.Services = services.BuildServiceProvider();
            blazorWebView1.RootComponents.Add<ConsoleLayout>("#app");
        }
    }
}