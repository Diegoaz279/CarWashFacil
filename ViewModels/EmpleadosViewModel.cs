using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CarWashFacil.ViewModels
{
    public partial class EmpleadosViewModel : BaseViewModel
    {
        private readonly EmpleadoService _empleadoService;
        private readonly AppStateService _appStateService;

        public ObservableCollection<Empleado> Empleados { get; } = new();

        [ObservableProperty]
        private string nombre = string.Empty;

        [ObservableProperty]
        private string telefono = string.Empty;

        [ObservableProperty]
        private bool activo = true;

        public EmpleadosViewModel(
            EmpleadoService empleadoService,
            AppStateService appStateService)
        {
            _empleadoService = empleadoService;
            _appStateService = appStateService;
            Titulo = "Empleados";

            // ⭐ SUSCRIPCIÓN: Recargar cuando otros notifiquen cambios
            _appStateService.DatosActualizados += async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await CargarInternoAsync();
                });
            };
        }

        /// <summary>
        /// ⭐ MÉTODO PÚBLICO: Para llamar desde OnAppearing de la página
        /// </summary>
        public async Task CargarAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                await CargarInternoAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// ⭐ MÉTODO PRIVADO: Recarga la lista sin mostrar loading
        /// </summary>
        private async Task CargarInternoAsync()
        {
            Empleados.Clear();
            var data = await _empleadoService.GetAllAsync();
            foreach (var item in data.OrderBy(x => x.Nombre))
                Empleados.Add(item);
        }

        [RelayCommand]
        private async Task GuardarEmpleado()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Nombre))
                {
                    await Shell.Current.DisplayAlert("Validación", "Ingrese el nombre del empleado.", "OK");
                    return;
                }

                bool confirmar = await Shell.Current.DisplayAlert(
                    "Guardar empleado",
                    $"¿Guardar a {Nombre.Trim()}?",
                    "Sí", "No");

                if (!confirmar) return;

                // GUARDAR
                await _empleadoService.AddAsync(new Empleado
                {
                    Nombre = Nombre.Trim(),
                    Telefono = Telefono.Trim(),
                    Activo = Activo
                });

                // LIMPIAR
                Nombre = string.Empty;
                Telefono = string.Empty;
                Activo = true;

                // ⭐ ACTUALIZACIÓN INMEDIATA: Recargar lista local
                await CargarInternoAsync();

                // ⭐ NOTIFICAR A OTROS: Para que otras páginas se actualicen
                _appStateService.NotificarCambios();

                await Shell.Current.DisplayAlert("Correcto", "Empleado guardado.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SeleccionarEmpleado(Empleado? empleado)
        {
            if (empleado == null || IsBusy) return;

            string accion = await Shell.Current.DisplayActionSheet(
                empleado.Nombre,
                "Cerrar",
                null,
                empleado.Activo ? "Desactivar" : "Activar",
                "Eliminar");

            if (accion == "Desactivar" || accion == "Activar")
            {
                empleado.Activo = !empleado.Activo;
                await _empleadoService.UpdateAsync(empleado);
                await CargarInternoAsync();           // ⭐ Recargar
                _appStateService.NotificarCambios();   // ⭐ Notificar
            }
            else if (accion == "Eliminar")
            {
                bool confirmar = await Shell.Current.DisplayAlert(
                    "Eliminar", $"¿Eliminar a {empleado.Nombre}?", "Sí", "No");

                if (confirmar)
                {
                    await _empleadoService.DeleteAsync(empleado);
                    await CargarInternoAsync();           // ⭐ Recargar
                    _appStateService.NotificarCambios();   // ⭐ Notificar
                }
            }
        }
    }
}