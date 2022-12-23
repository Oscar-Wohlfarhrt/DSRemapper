using DSRemapper.Configs;
using DSRemapper.DSInput;
using DSRemapper.DSOutput;

using System.Reflection;
using System.Runtime.Loader;
using System.CodeDom.Compiler;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.CSharp;

namespace DSRemapper.Remapper
{
    public class CSRemapper : IRemapper
    {
        readonly CompilerParameters parameters = new();
        public string ControllerId { get { return Controller.Id; } }
        public IDSInputController Controller { get; private set; }
        Func<DSInputReport, Action<string>, float, DSOutputReport>? remapFunction;
        AssemblyLoadContext remapContext = new("Remap", true);
        Assembly? program;
        private readonly RemapperScriptEventArgs eventArgs = new();
        private readonly RemapperReportEventArgs reportArgs = new();
        public event EventHandler<RemapperScriptEventArgs>? OnError;
        public event EventHandler<RemapperScriptEventArgs>? OnLog;
        public event EventHandler<RemapperReportEventArgs>? OnReportUpdate;
        public string LastProfile { get; private set; } = string.Empty;
        private readonly DSOutputController emuCtrls = new();
        private DSInputReport lastInput = new();

        private DateTime now = DateTime.Now, lastUpdate = DateTime.Now;
        float deltaTime = 0;

        public CSRemapper(IDSInputController controller, EventHandler<RemapperScriptEventArgs>? errHandler = null, EventHandler<RemapperScriptEventArgs>? logHandler = null, EventHandler<RemapperReportEventArgs>? updateHandler = null)
        {
            this.Controller = controller;
            reportArgs.id = eventArgs.id = controller.Id;

            if (errHandler != null)
                OnError += errHandler;
            if (logHandler != null)
                OnLog += logHandler;
            if (updateHandler != null)
                OnReportUpdate += updateHandler;

            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = true;
            parameters.ReferencedAssemblies.Add((Assembly.GetEntryAssembly() ??
                Assembly.GetAssembly(typeof(Program)) ?? Assembly.GetExecutingAssembly()).Location);
        }
        public void LoadScript(string profileName)
        {
            DisconnectEmulatedControllers();
            remapFunction = null;

            ProfileManager.SetLastProfile(ControllerId, profileName);
            LastProfile = profileName;

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(ProfileManager.GetProfileByName(profileName));

#pragma warning disable CS8604 // Posible argumento de referencia nulo
            string[] refPaths = new[] {
                typeof(object).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll"),
                (Assembly.GetEntryAssembly() ?? Assembly.GetAssembly(typeof(Program)) ?? Assembly.GetExecutingAssembly()).Location,
            };
#pragma warning restore CS8604 // Posible argumento de referencia nulo

            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            CSharpCompilation compilation = CSharpCompilation.Create(
                "RemapAssembly",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );

            MemoryStream ms = new();

            EmitResult result = compilation.Emit(ms);

            string compileMessage;

            if (!result.Success)
            {
                compileMessage = string.Join("\n", result.Diagnostics
                    .Where((d) => { return d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning; })
                    .Select((d) => { return d.GetMessage(); }));

                if (eventArgs.message != compileMessage)
                {
                    eventArgs.message = compileMessage;
                    OnError?.Invoke(this, eventArgs);
                }
            }
            else
            {
                if (result.Diagnostics.Length > 0)
                {
                    compileMessage = string.Join("\n", result.Diagnostics
                        .Where((d) => { return d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning; })
                        .Select((d) => { return d.GetMessage(); }));

                    if (eventArgs.message != compileMessage)
                    {
                        eventArgs.message = compileMessage;
                        OnError?.Invoke(this, eventArgs);
                    }
                }

                ms.Seek(0, SeekOrigin.Begin);

                if (remapContext.IsCollectible)
                {
                    remapContext.Unload();
                    remapContext = new("Remap", true);
                }

                program = remapContext.LoadFromStream(ms);
                program.GetType("RemapClass")?.GetMethod("Setup")?.CreateDelegate<Action<DSOutputController>>()?.Invoke(emuCtrls);

                Type? progType = program.GetType("RemapClass");
                MethodInfo? metInfo = progType?.GetMethod("Remap");
                remapFunction = metInfo?.CreateDelegate<Func<DSInputReport, Action<string>, float, DSOutputReport>>();
            }
        }
        public void ReloadScript()
        {
            LoadScript(LastProfile);
        }
        public void RemapController()
        {
            now = DateTime.Now;
            deltaTime = (now - lastUpdate).Ticks / (float)TimeSpan.TicksPerSecond;
            lastUpdate = now;

            try
            {
                lastInput = Controller.GetInputReport();
                reportArgs.report = lastInput;
                OnReportUpdate?.Invoke(this, reportArgs);
                if (remapFunction != null)
                {
                    Controller.SendOutputReport(remapFunction(lastInput, ConsoleLog, deltaTime));
                }
            }
            catch (Exception e)
            {
                if (eventArgs.message != e.Message)
                {
                    eventArgs.message = e.Message;
                    OnError?.Invoke(this, eventArgs);
                }
            }
        }
        public DSInputReport GetInputReport()
        {
            try
            {
                return lastInput;
            }
            catch { }

            return new DSInputReport();
        }
        public void Connect() => Controller.Connect();
        public void DisconnectEmulatedControllers() => emuCtrls.DisconnectAll();

        public void ConsoleLog(string message)
        {
            eventArgs.message = message;
            OnLog?.Invoke(this, eventArgs);
        }

        public void Dispose()
        {
            Controller.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}