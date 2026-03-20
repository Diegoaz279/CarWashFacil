using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace CarWashFacil.ViewModels
{
    /// <summary>
    /// REPORTES VIEWMODEL: Dashboard mensual con estadísticas.
    /// 
    /// GRID EN XAML:
    /// - Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto" = 4 cards en 2x2
    /// - Cards: Lavados | Ingresos | Gastos | Balance (el balance es morado destacado)
    /// 
    /// CICLO DE VIDA:
    /// - Se carga una vez al entrar (no hay actualización automática como en otras páginas)
    /// - MesActual muestra el mes actual en el header
    /// </summary>
    public partial class ReportesViewModel : BaseViewModel
    {
        private readonly LavadoService _lavadoService;
        private readonly CajaService _cajaService;

        // COLECCIÓN: Lista de operaciones del mes
        public ObservableCollection<Lavado> LavadosMes { get; } = new();

        // PROPIEDAD AÑADIDA: Para mostrar en header "Marzo 2026"
        [ObservableProperty]
        private DateTime mesActual = DateTime.Now;

        // PROPIEDADES KPI: Grid 2x2
        [ObservableProperty]
        private int totalLavadosMes;

        [ObservableProperty]
        private decimal ingresosMes;

        [ObservableProperty]
        private decimal gastosMes;

        [ObservableProperty]
        private decimal balanceMes;

        public ReportesViewModel(LavadoService lavadoService, CajaService cajaService)
        {
            _lavadoService = lavadoService;
            _cajaService = cajaService;
            Titulo = "Reportes";
        }

        /// <summary>
        /// MÉTODO: Carga datos del mes actual.
        /// Grid de KPIs y lista se actualizan con los resultados.
        /// </summary>
        public async Task CargarAsync()
        {
            LavadosMes.Clear();

            var all = await _lavadoService.GetAllAsync();
            var now = DateTime.Now;

            // Actualizar propiedad de fecha
            MesActual = now;

            var delMes = all.Where(x => x.Fecha.Month == now.Month && x.Fecha.Year == now.Year)
                            .OrderByDescending(x => x.Fecha)
                            .ToList();

            foreach (var item in delMes)
                LavadosMes.Add(item);

            // Cargar KPIs para Grid
            TotalLavadosMes = await _lavadoService.TotalLavadosMesAsync();
            IngresosMes = await _cajaService.TotalIngresosMesAsync();
            GastosMes = await _cajaService.TotalGastosMesAsync();
            BalanceMes = await _cajaService.BalanceMesAsync();
        }
    }
}