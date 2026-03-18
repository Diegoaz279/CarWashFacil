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

        public EmpleadosViewModel(EmpleadoService empleadoService, AppStateService appStateService)
        {
            _empleadoService = empleadoService;
            _appStateService = appStateService;

            Titulo = "Empleados";

            _appStateService.DatosActualizados += async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await CargarAsync();
                });
            };
        }

        public async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                Empleados.Clear();

                var data = await _empleadoService.GetAllAsync();
                foreach (var item in data.OrderBy(x => x.Nombre))
                    Empleados.Add(item);
            }
            catch (Exception ex)
            {
                await MostrarMensajeAsync("Error", $"No se pudieron cargar los empleados.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
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
                    await MostrarMensajeAsync("Validación", "Ingrese el nombre del empleado.");
                    return;
                }

                bool confirmar = await ConfirmarAsync(
                    "Guardar empleado",
                    $"¿Deseas guardar a {Nombre.Trim()}?"
                );

                if (!confirmar)
                    return;

                await _empleadoService.AddAsync(new Empleado
                {
                    Nombre = Nombre.Trim(),
                    Telefono = Telefono.Trim(),
                    Activo = Activo
                });

                Nombre = string.Empty;
                Telefono = string.Empty;
                Activo = true;

                _appStateService.NotificarCambios();

                await MostrarMensajeAsync("Correcto", "Empleado guardado.");
            }
            catch (Exception ex)
            {
                await MostrarMensajeAsync("Error", $"No se pudo guardar el empleado.\n{ex.Message}");
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

            try
            {
                string accion = await MostrarAccionesEmpleadoAsync(empleado);

                switch (accion)
                {
                    case "Desactivar":
                        await CambiarEstadoAsync(empleado, false);
                        break;

                    case "Activar":
                        await CambiarEstadoAsync(empleado, true);
                        break;

                    case "Eliminar":
                        await EliminarAsync(empleado);
                        break;
                }
            }
            catch (Exception ex)
            {
                await MostrarMensajeAsync("Error", $"No se pudo procesar la acción.\n{ex.Message}");
            }
        }

        private async Task CambiarEstadoAsync(Empleado empleado, bool nuevoEstado)
        {
            bool confirmar = await ConfirmarAsync(
                "Confirmar",
                nuevoEstado
                    ? $"¿Deseas activar a {empleado.Nombre}?"
                    : $"¿Deseas desactivar a {empleado.Nombre}?"
            );

            if (!confirmar) return;

            empleado.Activo = nuevoEstado;
            await _empleadoService.UpdateAsync(empleado);

            _appStateService.NotificarCambios();
        }

        private async Task EliminarAsync(Empleado empleado)
        {
            bool confirmar = await ConfirmarAsync(
                "Eliminar",
                $"¿Deseas eliminar a {empleado.Nombre}?"
            );

            if (!confirmar) return;

            await _empleadoService.DeleteAsync(empleado);
            _appStateService.NotificarCambios();
        }

        private async Task<string> MostrarAccionesEmpleadoAsync(Empleado empleado)
        {
            string accionEstado = empleado.Activo ? "Desactivar" : "Activar";

            if (Shell.Current != null)
            {
                return await Shell.Current.DisplayActionSheet(
                    empleado.Nombre,
                    "Cerrar",
                    null,
                    accionEstado,
                    "Eliminar");
            }

            return "Cerrar";
        }

        private async Task MostrarMensajeAsync(string titulo, string mensaje)
        {
            try
            {
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert(titulo, mensaje, "OK");
            }
            catch { }
        }

        private async Task<bool> ConfirmarAsync(string titulo, string mensaje)
        {
            try
            {
                if (Shell.Current != null)
                    return await Shell.Current.DisplayAlert(titulo, mensaje, "Sí", "No");
            }
            catch { }

            return false;
        }
    }
}