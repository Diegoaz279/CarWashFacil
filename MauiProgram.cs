using Microsoft.Extensions.Logging;

namespace CarWashFacil
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //SINGLETON (estado global)
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AppStateService>();
            builder.Services.AddSingleton<LifecycleService>();

            //SCOPED (lógica de negocio controlada por flujo)
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<LavadoService>();

            //TRANSIENT (servicios simples)
            builder.Services.AddTransient<EmpleadoService>();
            builder.Services.AddTransient<CajaService>();

            //VIEWMODELS (Transient para evitar estado sucio)
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<LavadosViewModel>();
            builder.Services.AddTransient<EmpleadosViewModel>();
            builder.Services.AddTransient<CajaViewModel>();
            builder.Services.AddTransient<ReportesViewModel>();
            builder.Services.AddTransient<EstadosViewModel>();

            //PAGES (Transient)
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<LavadosPage>();
            builder.Services.AddTransient<EmpleadosPage>();
            builder.Services.AddTransient<CajaPage>();
            builder.Services.AddTransient<ReportesPage>();
            builder.Services.AddTransient<EstadosPage>();

            builder.Services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}