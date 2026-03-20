using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CarWashFacil.ViewModels
{
    /// <summary>
    /// HOME VIEWMODEL: Dashboard principal con métricas del día.
    /// 
    /// GRID EN XAML:
    /// - HomePage.xaml usa Grid con ColumnDefinitions="*,*" RowDefinitions="Auto,Auto"
    /// - Esto crea un grid 2x2 para los KPI cards (Lavados, Ingresos, Gastos, Balance)
    /// - Grid.ColumnSpacing/RowSpacing controlan el espacio entre cards
    /// 
    /// CICLO DE VIDA:
    /// - AppStateService notifica cambios cuando otros ViewModels modifican datos
    /// - Se suscribe a DatosActualizados para refrescar automáticamente
    /// </summary>
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly LavadoService _lavadoService;
        private readonly CajaService _cajaService;
        private readonly AppStateService _appStateService;

        // PROPIEDADES BINDING: Grid de KPIs en HomePage.xaml
        [ObservableProperty]
        private int lavadosHoy;

        [ObservableProperty]
        private decimal ingresosHoy;

        [ObservableProperty]
        private decimal gastosHoy;

        [ObservableProperty]
        private decimal balanceHoy;

        /// <summary>
        /// PROPIEDAD AÑADIDA: FechaHoy para mostrar en el header del dashboard.
        /// Binding: {Binding FechaHoy, StringFormat='{0:dddd, dd MMMM yyyy}'}
        /// </summary>
        [ObservableProperty]
        private DateTime fechaHoy = DateTime.Now;

        /// <summary>
        /// CONSTRUCTOR DI: Servicios inyectados para cálculo de métricas.
        /// </summary>
        public HomeViewModel(
            LavadoService lavadoService,
            CajaService cajaService,
            AppStateService appStateService)
        {
            _lavadoService = lavadoService;
            _cajaService = cajaService;
            _appStateService = appStateService;

            Titulo = "Inicio";

            // PATRÓN OBSERVER: Cuando otros ViewModels llaman _appStateService.NotificarCambios()
            // este ViewModel se actualiza automáticamente (ciclo de vida reactivo)
            _appStateService.DatosActualizados += async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await CargarAsync();
                });
            };
        }

        /// <summary>
        /// MÉTODO: Carga métricas del día desde servicios.
        /// Se ejecuta en OnAppearing de la página y cuando hay cambios.
        /// </summary>
        public async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                LavadosHoy = await _lavadoService.TotalLavadosHoyAsync();
                IngresosHoy = await _cajaService.TotalIngresosHoyAsync();
                GastosHoy = await _cajaService.TotalGastosHoyAsync();
                BalanceHoy = await _cajaService.BalanceHoyAsync();

                // Actualizar fecha por si cambia mientras la app está abierta
                FechaHoy = DateTime.Now;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// COMMAND: Navegación a LavadosPage.
        /// Grid de destino usa ColumnDefinitions para formulario 2 columnas.
        /// </summary>
        [RelayCommand]
        private async Task IrALavados()
        {
            if (Shell.Current != null)
                await Shell.Current.GoToAsync("//Lavados");
        }

        /// <summary>
        /// COMMAND: Navegación a CajaPage.
        /// Grid de destino usa RowDefinitions="Auto,*" para header fijo + lista scrollable.
        /// </summary>
        [RelayCommand]
        private async Task IrACaja()
        {
            if (Shell.Current != null)
                await Shell.Current.GoToAsync("//Caja");
        }
    }
}