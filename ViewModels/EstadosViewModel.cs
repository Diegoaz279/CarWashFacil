using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CarWashFacil.ViewModels
{
    /// <summary>
    /// ESTADOS VIEWMODEL: Log de eventos del sistema y ciclo de vida.
    /// 
    /// GRID EN XAML:
    /// - Grid ColumnDefinitions="4,*": 
    ///   - Column 0: BoxView de 4px de ancho como indicador de color
    ///   - Column 1: Frame con información del evento
    /// - Grid RowDefinitions="Auto,*" en página completa
    /// 
    /// CICLO DE VIDA APP (ESTE ES EL MÁS IMPORTANTE PARA TU PROYECTO):
    /// - OnStart: Se registra cuando la app inicia
    /// - OnSleep: App va a background (pausada)
    /// - OnResume: App vuelve de background (reanudada)
    /// - LifecycleService captura todos estos eventos automáticamente
    /// </summary>
    public partial class EstadosViewModel : BaseViewModel
    {
        private readonly LifecycleService _lifecycleService;

        // COLECCIÓN: Eventos de ciclo de vida y operaciones
        public ObservableCollection<EventoSistema> Eventos { get; } = new();

        public EstadosViewModel(LifecycleService lifecycleService)
        {
            _lifecycleService = lifecycleService;
            Titulo = "Estados";
        }

        /// <summary>
        /// MÉTODO: Carga historial de eventos del ciclo de vida.
        /// Muestra: Inicio app, Pausas, Reanudaciones, Login, Errores, etc.
        /// </summary>
        public async Task CargarAsync()
        {
            Eventos.Clear();
            var data = await _lifecycleService.GetEventsAsync();

            foreach (var item in data)
                Eventos.Add(item);
        }

        /// <summary>
        /// COMMAND: Limpia el log de eventos.
        /// Útil para debugging del ciclo de vida.
        /// </summary>
        [RelayCommand]
        private async Task Limpiar()
        {
            await _lifecycleService.ClearAsync();
            await CargarAsync();
        }
    }
}

/// <summary>
/// MODELO AUXILIAR: Representa un evento del sistema.
/// Tipo: Info, Exito, Advertencia, Error (para colorear el Grid indicador)
/// </summary>
public class EventoSistema
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Info"; // Info, Exito, Advertencia, Error
    public DateTime Fecha { get; set; } = DateTime.Now;


    // Propiedad calculada para binding
    public string FechaTexto => Fecha.ToString("dd/MM/yyyy hh:mm:ss tt");

    // Icono según tipo (para mostrar en XAML)
    public string Icono => Tipo switch
    {
        "Exito" => "✅",
        "Advertencia" => "⏸️",
        "Window" => "🪟",
        "Error" => "❌",
        _ => "ℹ️"
    };

    // Color según tipo (para BoxView indicador)
    public string TipoColor => Tipo switch
    {
        "Error" => "#EF4444",
        "Advertencia" => "#F59E0B",
        "Exito" => "#10B981",
        _ => "#2563EB"
    };
}