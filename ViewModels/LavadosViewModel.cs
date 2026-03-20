using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CarWashFacil.ViewModels
{
    /// <summary>
    /// LAVADOS VIEWMODEL: Gestión completa de servicios de lavado.
    /// 
    /// GRID AVANZADO EN XAML:
    /// - Formulario: Grid ColumnDefinitions="*,*" para 2 columnas (Cliente/Placa, Tipo/Precio)
    /// - Lista items: Grid ColumnDefinitions="Auto,*,Auto" para icono + info + precio
    /// - Lista items: RowDefinitions="Auto,Auto,Auto" para apilar verticalmente los datos
    /// 
    /// CICLO DE VIDA APP:
    /// - OnSleep: Guarda estado del formulario parcial (no implementado, pero podría)
    /// - OnResume: Recarga datos por si cambiaron en background
    /// - AppStateService: Notifica a otros ViewModels cuando hay cambios (Caja, Home)
    /// </summary>
    public partial class LavadosViewModel : BaseViewModel
    {
        private readonly LavadoService _lavadoService;
        private readonly CajaService _cajaService;
        private readonly EmpleadoService _empleadoService;
        private readonly AppStateService _appStateService;

        // COLECCIONES: Bindings a CollectionView en XAML
        public ObservableCollection<Lavado> LavadosPendientes { get; } = new();
        public ObservableCollection<Lavado> LavadosTerminadosHoy { get; } = new();
        public ObservableCollection<Empleado> Empleados { get; } = new();

        // LISTAS ESTÁTICAS: Opciones para Pickers en XAML
        public ObservableCollection<string> TiposLavado { get; } = new()
        {
            "Básico",
            "Premium",
            "Motor",
            "Completo"
        };

        public ObservableCollection<string> Estados { get; } = new()
        {
            "Pendiente",
            "Terminado",
            "Cancelado"
        };

        // PROPIEDADES FORMULARIO: Bindings TwoWay a Entries/Pickers
        [ObservableProperty]
        private string cliente = string.Empty;

        [ObservableProperty]
        private string placa = string.Empty;

        [ObservableProperty]
        private string tipoLavado = "Básico";

        [ObservableProperty]
        private decimal precio = 350;

        [ObservableProperty]
        private string estado = "Pendiente";

        [ObservableProperty]
        private Empleado? empleadoSeleccionado;

        [ObservableProperty]
        private Lavado? lavadoSeleccionado;

        [ObservableProperty]
        private string detalleLavadoSeleccionado = "Toque un lavado para ver opciones.";

        // PROPIEDADES KPI: Mostradas en Grid superior de 2 columnas
        [ObservableProperty]
        private int totalPendientesHoy;

        [ObservableProperty]
        private int totalTerminadosHoy;

        public LavadosViewModel(
            LavadoService lavadoService,
            CajaService cajaService,
            EmpleadoService empleadoService,
            AppStateService appStateService)
        {
            _lavadoService = lavadoService;
            _cajaService = cajaService;
            _empleadoService = empleadoService;
            _appStateService = appStateService;

            Titulo = "Lavados";

            // CICLO DE VIDA: Suscripción a eventos globales de actualización
            _appStateService.DatosActualizados += async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await RecargarDesdeEventosAsync();
                });
            };
        }

        /// <summary>
        /// PARTIAL METHOD: Se ejecuta automáticamente cuando cambia TipoLavado.
        /// Actualiza el precio base según el tipo seleccionado.
        /// </summary>
        partial void OnTipoLavadoChanged(string value)
        {
            // Solo actualiza si el precio es uno de los valores por defecto
            if (Precio <= 0 || Precio == 350 || Precio == 700 || Precio == 500 || Precio == 900)
            {
                Precio = ObtenerPrecioBase(value);
            }
        }

        /// <summary>
        /// PARTIAL METHOD: Actualiza el detalle mostrado cuando se selecciona un lavado.
        /// Binding: Label en XAML con texto multilinea (Grid.Row múltiple).
        /// </summary>
        partial void OnLavadoSeleccionadoChanged(Lavado? value)
        {
            if (value == null)
            {
                DetalleLavadoSeleccionado = "Toque un lavado para ver opciones.";
                return;
            }

            // Grid en XAML muestra esto en múltiples líneas usando LineBreakMode
            DetalleLavadoSeleccionado =
                $"Cliente: {value.Cliente}\n" +
                $"Placa: {value.Placa}\n" +
                $"Tipo: {value.TipoLavado}\n" +
                $"Empleado: {value.EmpleadoNombre}\n" +
                $"Precio: RD${value.Precio:F2}\n" +
                $"Estado: {value.Estado}\n" +
                $"Fecha: {value.Fecha:dd/MM/yyyy hh:mm tt}";
        }

        /// <summary>
        /// MÉTODO PÚBLICO: Carga inicial desde OnAppearing de la página.
        /// Ciclo de vida: Se ejecuta cada vez que la página aparece (navegación).
        /// </summary>
        public async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await CargarInternoAsync();
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("No se pudieron cargar los lavados.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// MÉTODO PRIVADO: Recarga silenciosa desde eventos de otro ViewModel.
        /// Ciclo de vida: No muestra loading ni errores (background update).
        /// </summary>
        private async Task RecargarDesdeEventosAsync()
        {
            try
            {
                await CargarInternoAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al recargar lavados: {ex}");
            }
        }

        /// <summary>
        /// LÓGICA DE CARGA: Llena las colecciones ObservableCollection.
        /// Grid de KPIs se actualiza automáticamente al cambiar TotalPendientesHoy/TotalTerminadosHoy.
        /// </summary>
        private async Task CargarInternoAsync()
        {
            LavadosPendientes.Clear();
            LavadosTerminadosHoy.Clear();
            Empleados.Clear();

            var empleados = await _empleadoService.GetActivosAsync();
            foreach (var empleado in empleados)
                Empleados.Add(empleado);

            var lavados = await _lavadoService.GetAllAsync();
            var hoy = DateTime.Today;

            foreach (var lavado in lavados.OrderByDescending(x => x.Fecha))
            {
                if (lavado.Estado == "Pendiente")
                    LavadosPendientes.Add(lavado);

                if (lavado.Estado == "Terminado" && lavado.Fecha.Date == hoy)
                    LavadosTerminadosHoy.Add(lavado);
            }

            TotalPendientesHoy = LavadosPendientes.Count(x => x.Fecha.Date == hoy);
            TotalTerminadosHoy = LavadosTerminadosHoy.Count;
        }

        /// <summary>
        /// COMMAND: Guarda nuevo lavado y notifica a otros ViewModels.
        /// Grid del formulario se limpia después de guardar.
        /// </summary>
        [RelayCommand]
        private async Task GuardarLavado()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var mensajeValidacion = ValidarFormulario();
                if (!string.IsNullOrWhiteSpace(mensajeValidacion))
                {
                    await MostrarMensajeAsync("Validación", mensajeValidacion);
                    return;
                }

                bool confirmar = await ConfirmarAsync(
                    "Confirmar lavado",
                    $"¿Deseas guardar el lavado de la placa {Placa.Trim().ToUpper()} por RD${Precio:F2}?"
                );

                if (!confirmar)
                    return;

                var lavado = new Lavado
                {
                    Cliente = Cliente.Trim(),
                    Placa = Placa.Trim().ToUpper(),
                    TipoLavado = TipoLavado,
                    Precio = Precio,
                    Estado = Estado,
                    Fecha = DateTime.Now,
                    EmpleadoId = EmpleadoSeleccionado!.Id,
                    EmpleadoNombre = EmpleadoSeleccionado.Nombre
                };

                await _lavadoService.AddAsync(lavado);

                // Si se crea como terminado, genera ingreso en caja automáticamente
                if (lavado.Estado == "Terminado")
                {
                    await _cajaService.AddIngresoAsync($"Lavado {lavado.Placa}", lavado.Precio);
                }

                LimpiarFormulario();

                // CICLO DE VIDA: Recarga local inmediata
                await CargarInternoAsync();

                // CICLO DE VIDA: Notifica a Home y Caja para que se actualicen
                _appStateService.NotificarCambios();

                await MostrarMensajeAsync("Correcto", "Lavado guardado correctamente.");
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("No se pudo guardar el lavado.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// COMMAND: Selección de lavado desde CollectionView (Grid item).
        /// Muestra ActionSheet con opciones según estado.
        /// </summary>
        [RelayCommand]
        private async Task SeleccionarLavado(Lavado? lavado)
        {
            if (lavado == null || IsBusy) return;

            LavadoSeleccionado = lavado;

            try
            {
                string accion = await MostrarAccionesLavadoAsync(lavado);

                switch (accion)
                {
                    case "Terminar y cobrar":
                        await TerminarLavadoInternoAsync(lavado);
                        break;

                    case "Cambiar precio":
                        await CambiarPrecioInternoAsync(lavado);
                        break;

                    case "Cancelar":
                        await CancelarLavadoInternoAsync(lavado);
                        break;
                }
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("No se pudo procesar la acción del lavado.", ex);
            }
        }

        [RelayCommand]
        private async Task MarcarTerminado()
        {
            if (LavadoSeleccionado == null)
            {
                await MostrarMensajeAsync("Aviso", "Seleccione un lavado pendiente.");
                return;
            }

            await TerminarLavadoInternoAsync(LavadoSeleccionado);
        }

        [RelayCommand]
        private async Task CancelarLavado()
        {
            if (LavadoSeleccionado == null)
            {
                await MostrarMensajeAsync("Aviso", "Seleccione un lavado pendiente.");
                return;
            }

            await CancelarLavadoInternoAsync(LavadoSeleccionado);
        }

        [RelayCommand]
        private async Task Refrescar()
        {
            await CargarAsync();
        }

        [RelayCommand]
        private void LimpiarFormularioCommand()
        {
            LimpiarFormulario();
        }

        // MÉTODOS PRIVADOS: Lógica de negocio

        private async Task TerminarLavadoInternoAsync(Lavado lavado)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                bool confirmar = await ConfirmarAsync(
                    "Confirmar cobro",
                    $"¿Deseas terminar y cobrar el lavado de {lavado.Placa} por RD${lavado.Precio:F2}?"
                );

                if (!confirmar)
                    return;

                lavado.Estado = "Terminado";

                await _lavadoService.UpdateAsync(lavado);
                await _cajaService.AddIngresoAsync($"Lavado {lavado.Placa}", lavado.Precio);

                await CargarInternoAsync();
                _appStateService.NotificarCambios();

                await MostrarMensajeAsync("Correcto", "Lavado terminado y cobrado.");
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("No se pudo terminar el lavado.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelarLavadoInternoAsync(Lavado lavado)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                bool confirmar = await ConfirmarAsync(
                    "Cancelar lavado",
                    $"¿Deseas cancelar el lavado de la placa {lavado.Placa}?"
                );

                if (!confirmar)
                    return;

                lavado.Estado = "Cancelado";
                await _lavadoService.UpdateAsync(lavado);

                await CargarInternoAsync();
                _appStateService.NotificarCambios();

                await MostrarMensajeAsync("Correcto", "Lavado cancelado.");
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("No se pudo cancelar el lavado.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CambiarPrecioInternoAsync(Lavado lavado)
        {
            try
            {
                string? nuevoTexto = await MostrarPromptAsync(
                    "Editar precio",
                    $"Precio actual: RD${lavado.Precio:F2}\nEscribe el nuevo monto:"
                );

                if (string.IsNullOrWhiteSpace(nuevoTexto))
                    return;

                if (!decimal.TryParse(nuevoTexto, out decimal nuevoPrecio) || nuevoPrecio <= 0)
                {
                    await MostrarMensajeAsync("Validación", "Ingrese un precio válido mayor que cero.");
                    return;
                }

                bool confirmar = await ConfirmarAsync(
                    "Confirmar precio",
                    $"¿Deseas cambiar el precio a RD${nuevoPrecio:F2}?"
                );

                if (!confirmar)
                    return;

                lavado.Precio = nuevoPrecio;
                await _lavadoService.UpdateAsync(lavado);

                await CargarInternoAsync();
                _appStateService.NotificarCambios();

                await MostrarMensajeAsync("Correcto", "Precio actualizado.");
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("No se pudo cambiar el precio.", ex);
            }
        }

        private decimal ObtenerPrecioBase(string tipoLavado)
        {
            return tipoLavado switch
            {
                "Básico" => 350,
                "Premium" => 700,
                "Motor" => 500,
                "Completo" => 900,
                _ => 350
            };
        }

        private string ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(Cliente))
                return "Debe ingresar el nombre del cliente.";

            if (string.IsNullOrWhiteSpace(Placa))
                return "Debe ingresar la placa.";

            if (Placa.Trim().Length < 3)
                return "La placa debe tener al menos 3 caracteres.";

            if (Precio <= 0)
                return "El precio debe ser mayor que cero.";

            if (EmpleadoSeleccionado == null)
                return "Debe seleccionar un empleado.";

            return string.Empty;
        }

        private void LimpiarFormulario()
        {
            Cliente = string.Empty;
            Placa = string.Empty;
            TipoLavado = "Básico";
            Precio = 350;
            Estado = "Pendiente";
            EmpleadoSeleccionado = null;
        }

        // HELPERS UI

        private async Task<string> MostrarAccionesLavadoAsync(Lavado lavado)
        {
            if (Shell.Current != null)
            {
                return await Shell.Current.DisplayActionSheet(
                    $"Lavado {lavado.Placa}",
                    "Cerrar",
                    null,
                    "Terminar y cobrar",
                    "Cambiar precio",
                    "Cancelar");
            }

            return "Cerrar";
        }

        private async Task MostrarMensajeAsync(string titulo, string mensaje)
        {
            try
            {
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert(titulo, mensaje, "OK");
                else if (Application.Current?.Windows.Count > 0)
                    await Application.Current.Windows[0].Page!.DisplayAlert(titulo, mensaje, "OK");
            }
            catch { }
        }

        private async Task<bool> ConfirmarAsync(string titulo, string mensaje)
        {
            try
            {
                if (Shell.Current != null)
                    return await Shell.Current.DisplayAlert(titulo, mensaje, "Sí", "No");

                if (Application.Current?.Windows.Count > 0)
                    return await Application.Current.Windows[0].Page!.DisplayAlert(titulo, mensaje, "Sí", "No");
            }
            catch { }

            return false;
        }

        private async Task<string?> MostrarPromptAsync(string titulo, string mensaje)
        {
            try
            {
                if (Shell.Current != null)
                    return await Shell.Current.DisplayPromptAsync(titulo, mensaje, initialValue: "", maxLength: 10, keyboard: Keyboard.Numeric);

                if (Application.Current?.Windows.Count > 0)
                    return await Application.Current.Windows[0].Page!.DisplayPromptAsync(titulo, mensaje, initialValue: "", maxLength: 10, keyboard: Keyboard.Numeric);
            }
            catch { }

            return null;
        }

        private async Task MostrarErrorAsync(string mensajeBase, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error LavadosViewModel: {ex}");
            await MostrarMensajeAsync("Error", $"{mensajeBase}\n{ex.Message}");
        }
    }
}