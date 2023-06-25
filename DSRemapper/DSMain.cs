using DSRemapper.RazorLayouts;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using System.Runtime.InteropServices.ObjectiveC;

namespace DSRemapper
{
    public partial class DSMain : Form
    {
        public DSMain(bool def = true)
        {
            InitializeComponent();

            if (def)
            {
                var services = new ServiceCollection();
                services.AddWindowsFormsBlazorWebView();
                blazorWebView1.HostPage = "wwwroot\\index.html";
                blazorWebView1.Services = services.BuildServiceProvider();
                blazorWebView1.RootComponents.Add<MainLayout>("#app");
            }
        }
    }
    public class DSForm<T> : DSMain where T : IComponent
    {
        public DSForm(IDictionary<string, object?>? parameters = null) : base(false)
        {
            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();
            blazorWebView1.HostPage = "wwwroot\\index.html";
            blazorWebView1.Services = services.BuildServiceProvider();
            blazorWebView1.RootComponents.Add<T>("#app", parameters);
        }
    }
}