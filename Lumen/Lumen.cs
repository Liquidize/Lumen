using Lumen.Api.Graphics;
using Lumen.Network;
using Lumen.Registries;
using Lumen.Server;
using Lumen.Web;
using Serilog;

namespace Lumen
{
    public class Lumen
    {
        private static bool _exiting = false;

        private static IHost _host;

        public static CanvasRegistry CanvasRegistry { get; private set; }
        public static EffectRegistry EffectRegistry { get; private set; }
        public static LocationRegistry LocationRegistry { get; private set; }


        static void Main(string[] args)
        {
           Initialize(args);

            // Main app loop to keep the app running
            while (!_exiting)
            {
                if (Environment.UserInteractive && Console.In.Peek() > 0)
                {
                        if (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo info = Console.ReadKey();
                            if (info.KeyChar == 'c') Console.Clear();
                        }
                }
            }

            Console.WriteLine("Exiting...");
        }


        static void LoadAssemblies()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);

            var assemblyFiles = Directory.GetFiles(path, "*.dll");
            var loaded = 0;
            foreach (var file in assemblyFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    if (assembly != null) loaded++;
                }
                catch (Exception ex)
                {
                    // Might want to do more than just log? Possibly just halt operation as it may  cause issues down the line if a canvas or effect type cant be found?
                    Log.Error(ex, $"Unable to load assembly from path {file}.");
                }
            }

            Log.Information($"Loaded {loaded} of {assemblyFiles.Length} external Assemblies.");

        }


        static void Initialize(string[] args)
        {
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Loading Assemblies...");
            LoadAssemblies();

            Log.Information("Building Web Server...");

            _host = CreateHostBuilder(args).Build();

            CanvasRegistry = (Registries.CanvasRegistry)_host.Services.GetRequiredService<ICanvasRegistry>();
            EffectRegistry = (Registries.EffectRegistry)_host.Services.GetRequiredService<IEffectRegistry>();
            LocationRegistry = (Registries.LocationRegistry)_host.Services.GetRequiredService<ILocationRegistry>();


            _host.RunAsync();

            Log.Information("Loading effects..."); 
            EffectRegistry.LoadEffects();

            Log.Information("Loading canvases...");
            CanvasRegistry.LoadCanvases();

            Log.Information("Loading locations...");
            LocationRegistry.LoadLocations();


            if (Environment.UserInteractive)
                Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            

            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;


        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
          return  Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }

        private static async void CurrentDomainOnProcessExit(object? sender, EventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
        }


        private static void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _exiting = true;
            e.Cancel = false;
        }
    }
}
