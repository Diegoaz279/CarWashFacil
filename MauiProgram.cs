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

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AppStateService>();

            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<EmpleadoService>();
            builder.Services.AddSingleton<LavadoService>();
            builder.Services.AddSingleton<CajaService>();
            builder.Services.AddSingleton<LifecycleService>();

            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<LavadosViewModel>();
            builder.Services.AddSingleton<EmpleadosViewModel>();
            builder.Services.AddSingleton<CajaViewModel>();
            builder.Services.AddSingleton<ReportesViewModel>();
            builder.Services.AddSingleton<EstadosViewModel>();

            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<LavadosPage>();
            builder.Services.AddSingleton<EmpleadosPage>();
            builder.Services.AddSingleton<CajaPage>();
            builder.Services.AddSingleton<ReportesPage>();
            builder.Services.AddSingleton<EstadosPage>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}