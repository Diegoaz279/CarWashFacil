using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CarWashFacil.ViewModels
{
    /// <summary>
    /// LOGIN VIEWMODEL: Maneja autenticación y navegación inicial.
    /// 
    /// INYECCIÓN DE DEPENDENCIAS (DI):
    /// - AuthService: Singleton, persiste durante toda la app
    /// - LifecycleService: Singleton, registra eventos de ciclo de vida
    /// - IServiceProvider: Factory para crear AppShell (navegación post-login)
    /// 
    /// CICLO DE VIDA APLICACIÓN MÓVIL:
    /// - OnStart: Se ejecuta al abrir la app (MainPage = LoginPage)
    /// - OnResume: No aplica aquí, pero LifecycleService lo registra
    /// - OnSleep: App en background, LifecycleService guarda estado
    /// </summary>
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly LifecycleService _lifecycleService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string usuario = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        /// <summary>
        /// CONSTRUCTOR DI: Los servicios se inyectan automáticamente por MauiProgram.cs
        /// </summary>
        public LoginViewModel(
            AuthService authService,
            LifecycleService lifecycleService,
            IServiceProvider serviceProvider)
        {
            _authService = authService;
            _lifecycleService = lifecycleService;
            _serviceProvider = serviceProvider;
            Titulo = "Login";
        }

        /// <summary>
        /// COMMAND: IniciarSesion - Valida credenciales y navega al Shell principal.
        /// 
        /// GRID/XAML RELACIONADO:
        /// - LoginPage.xaml usa Grid para centrar el formulario verticalmente
        /// - Grid.RowDefinitions="Auto,*" separa header del formulario
        /// </summary>
        [RelayCommand]
        private async Task IniciarSesion()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // Validaciones de campos vacíos
                if (string.IsNullOrWhiteSpace(Usuario) && string.IsNullOrWhiteSpace(Password))
                {
                    await MostrarMensajeAsync("Validación", "Debe ingresar usuario y contraseña.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Usuario))
                {
                    await MostrarMensajeAsync("Validación", "Debe ingresar el usuario.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    await MostrarMensajeAsync("Validación", "Debe ingresar la contraseña.");
                    return;
                }

                var ok = await _authService.LoginAsync(Usuario.Trim(), Password.Trim());

                if (!ok)
                {
                    await MostrarMensajeAsync("Acceso denegado", "Usuario o contraseña incorrectos.");
                    return;
                }

                // REGISTRO CICLO DE VIDA: Evento de login exitoso
                _ = _lifecycleService.AddEventSafeAsync("Login exitoso");

                // NAVEGACIÓN: Reemplaza la ventana actual con AppShell (MainPage)
                // Esto destruye el LoginViewModel (Transient) y crea los demás ViewModels
                Application.Current!.Windows[0].Page = _serviceProvider.GetRequiredService<AppShell>();
            }
            catch (Exception ex)
            {
                await MostrarMensajeAsync("Error", $"Ocurrió un problema al iniciar sesión.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task MostrarMensajeAsync(string titulo, string mensaje)
        {
            try
            {
                if (Application.Current?.Windows.Count > 0)
                    await Application.Current.Windows[0].Page!.DisplayAlert(titulo, mensaje, "OK");
            }
            catch
            {
                // Silenciar errores de UI si la app está en background
            }
        }
    }
}