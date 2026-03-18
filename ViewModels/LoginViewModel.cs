namespace CarWashFacil.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly LifecycleService _lifecycleService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string usuario = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        public LoginViewModel(AuthService authService, LifecycleService lifecycleService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _lifecycleService = lifecycleService;
            _serviceProvider = serviceProvider;
            Titulo = "Login";
        }

        [RelayCommand]
        private async Task IniciarSesion()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

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

                _ = _lifecycleService.AddEventSafeAsync("Login exitoso");

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
            }
        }
    }
}