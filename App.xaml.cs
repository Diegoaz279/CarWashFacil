using Microsoft.Maui.Controls;

namespace CarWashFacil
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = default!;

        private readonly DatabaseService _databaseService;
        private readonly LifecycleService _lifecycleService;
        private readonly LoginPage _loginPage;

        public App(IServiceProvider serviceProvider,
                   DatabaseService databaseService,
                   LifecycleService lifecycleService,
                   LoginPage loginPage)
        {
            InitializeComponent();

            Services = serviceProvider;
            _databaseService = databaseService;
            _lifecycleService = lifecycleService;
            _loginPage = loginPage;

            MainPage = new NavigationPage(_loginPage);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(MainPage!);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await _databaseService.InitAsync();
                    _ = _lifecycleService.AddEventSafeAsync("🚀 App creada - CreateWindow ejecutado");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al iniciar DB: {ex}");
                }
            });

            // ⭐ SOLO eventos de Window que NO duplican OnSleep/OnResume
            // Activated/Deactivated son diferentes (foco de ventana vs. background)
            window.Activated += (s, e) =>
                _ = _lifecycleService.AddEventSafeAsync("🟢 Window Activated - Ventana con foco");

            window.Deactivated += (s, e) =>
                _ = _lifecycleService.AddEventSafeAsync("🟡 Window Deactivated - Ventana sin foco");

            // ❌ ELIMINADOS: window.Stopped y window.Resumed (duplicaban OnSleep/OnResume)

            return window;
        }

        // ⭐ MÉTODOS TRADICIONALES DEL CICLO DE VIDA (sin duplicar)
        protected override void OnStart()
        {
            base.OnStart();
            _ = _lifecycleService.AddEventSafeAsync("▶️ ON START - App iniciada");
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            _ = _lifecycleService.AddEventSafeAsync("💤 ON SLEEP - App en segundo plano");
        }

        protected override void OnResume()
        {
            base.OnResume();
            _ = _lifecycleService.AddEventSafeAsync("🌅 ON RESUME - App reanudada");
        }
    }
}