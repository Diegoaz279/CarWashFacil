using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CarWashFacil.ViewModels
{
    /// <summary>
    /// CAJA VIEWMODEL: Control de ingresos y gastos del día.
    /// 
    /// GRID EN XAML:
    /// - Grid RowDefinitions="Auto,*": 
    ///   - Row 0: ScrollView con formulario y KPIs (tamaño automático)
    ///   - Row 1: CollectionView de movimientos (ocupa resto de pantalla)
    /// - Grid ColumnDefinitions="*,*" para Ingresos/Gastos lado a lado
    /// 
    /// CICLO DE VIDA APP:
    /// - OnSleep: Podría guardar transacción pendiente (no implementado)
    /// - OnResume: Recarga datos por si hubo cambios en background
    /// - AppStateService: Recibe notificaciones de LavadosViewModel (nuevos ingresos)
    /// </summary>
    public partial class CajaViewModel : BaseViewModel
    {
        private readonly CajaService _cajaService;
        private readonly AppStateService _appStateService;

        // COLECCIÓN: Binding a CollectionView en Grid.Row="1"
        public ObservableCollection<MovimientoCaja> Movimientos { get; } = new();

        // PROPIEDADES FORMULARIO
        [ObservableProperty]
        private string descripcion = string.Empty;

        [ObservableProperty]
        private decimal monto;

        // PROPIEDADES KPI: Grid superior con 2 columnas + 1 fila doble (balance)
        [ObservableProperty]
        private decimal ingresosHoy;

        [ObservableProperty]
        private decimal gastosHoy;

        [ObservableProperty]
        private decimal balanceHoy;

        public CajaViewModel(CajaService cajaService, AppStateService appStateService)
        {
            _cajaService = cajaService;
            _appStateService = appStateService;
            Titulo = "Caja";

            // CICLO DE VIDA: Suscripción a eventos globales
            _appStateService.DatosActualizados += async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await CargarAsync();
                });
            };
        }

        /// <summary>
        /// MÉTODO: Carga movimientos y calcula KPIs.
        /// Grid de KPIs se actualiza al cambiar IngresosHoy/GastosHoy/BalanceHoy.
        /// </summary>
        public async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                Movimientos.Clear();

                var data = await _cajaService.GetAllAsync();
                foreach (var item in data.OrderByDescending(x => x.Fecha))
                    Movimientos.Add(item);

                // Cálculos para Grid de KPIs
                IngresosHoy = await _cajaService.TotalIngresosHoyAsync();
                GastosHoy = await _cajaService.TotalGastosHoyAsync();
                BalanceHoy = await _cajaService.BalanceHoyAsync();
            }
            catch (Exception ex)
            {
                await MostrarMensajeAsync("Error", $"No se pudo cargar la caja.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// COMMAND: Registra gasto y actualiza Grid de KPIs.
        /// </summary>
        [RelayCommand]
        private async Task RegistrarGasto()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Descripcion))
                {
                    await MostrarMensajeAsync("Validación", "Debe escribir la descripción del gasto.");
                    return;
                }

                if (Monto <= 0)
                {
                    await MostrarMensajeAsync("Validación", "El monto debe ser mayor que cero.");
                    return;
                }

                bool confirmar = await ConfirmarAsync(
                    "Confirmar gasto",
                    $"¿Deseas registrar el gasto \"{Descripcion.Trim()}\" por RD${Monto:F2}?"
                );

                if (!confirmar)
                    return;

                await _cajaService.AddGastoAsync(Descripcion.Trim(), Monto);

                // Limpiar formulario
                Descripcion = string.Empty;
                Monto = 0;

                _appStateService.NotificarCambios();

                await MostrarMensajeAsync("Correcto", "Gasto registrado correctamente.");
            }
            catch (Exception ex)
            {
                await MostrarMensajeAsync("Error", $"No se pudo registrar el gasto.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Refrescar()
        {
            await CargarAsync();
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
    }
}