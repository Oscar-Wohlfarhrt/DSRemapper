using DSRemapper.RazorLayouts;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using System.Runtime.InteropServices.ObjectiveC;
using DSRemapper.Core;

namespace DSRemapper
{
    /// <summary>
    /// Main form of the DSRemapper Application
    /// </summary>
    public partial class DSMain : Form
    {
        /// <summary>
        /// DSMain class contructor
        /// </summary>
        /// <param name="def"></param>
        public DSMain(bool def = true)
        {
            InitializeComponent();

            if (def)
            {
                var services = new ServiceCollection();
                services.AddWindowsFormsBlazorWebView();
                blazorWebView1.HostPage = Path.Combine(DSPaths.ProgramPath,"wwwroot\\index.html");
                blazorWebView1.Services = services.BuildServiceProvider();
                blazorWebView1.RootComponents.Add<MainLayout>("#app");
            }
        }
    }
    /// <summary>
    /// Generic form of the DSMain class/form.
    /// </summary>
    /// <typeparam name="T">Razor Layout to be rendered by this form</typeparam>
    public class DSForm<T> : DSMain where T : IComponent
    {
        /// <summary>
        /// DSForm class contructor
        /// </summary>
        /// <param name="parameters">Razor parameters for the Razor Layout</param>
        public DSForm(IDictionary<string, object?>? parameters = null) : base(false)
        {
            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();
            blazorWebView1.HostPage = Path.Combine(DSPaths.ProgramPath, "wwwroot\\index.html");
            blazorWebView1.Services = services.BuildServiceProvider();
            blazorWebView1.RootComponents.Add<T>("#app", parameters);
        }
    }
}