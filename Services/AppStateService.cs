namespace CarWashFacil.Services
{
    /// <summary>
    /// APP STATE SERVICE: Servicio Singleton de estado global y comunicación entre componentes.
    /// 
    /// PATRÓN: Observer (Publish-Subscribe) / Event Aggregator
    /// RESPONSABILIDAD: Coordinar notificaciones entre ViewModels desacoplados
    /// 
    /// PROBLEMA QUE RESUELVE:
    /// - En MVVM, ViewModels no deben conocerse directamente (acoplamiento)
    /// - Pero necesitan reaccionar a cambios en otros módulos
    ///   (Ej: LavadosViewModel guarda → HomeViewModel y CajaViewModel deben refrescar)
    /// 
    /// SOLUCIÓN: Evento centralizado donde publicadores y suscriptores son anónimos
    /// 
    /// CICLO DE VIDA:
    /// - Singleton: Mismo evento disponible en toda la aplicación
    /// - Los suscriptores se registran en constructores de ViewModels
    /// - Los suscriptores DEBEN desregistrarse al destruirse (memory leaks)
    /// </summary>
    public class AppStateService
    {
        // ===================================================================
        // EVENTO: Mecanismo de notificación pub-sub
        // ===================================================================

        /// <summary>
        /// EVENTO: DatosActualizados
        /// 
        /// TIPO: Action? (delegate sin parámetros, nullable)
        /// 
        /// SUSCRIPTORES TÍPICOS:
        /// - HomeViewModel: Refresca KPIs del dashboard
        /// - CajaViewModel: Recalcula balance e ingresos
        /// - ReportesViewModel: Actualiza estadísticas mensuales
        /// 
        /// DISPARADORES TÍPICOS:
        /// - LavadosViewModel: Al guardar/terminar/cancelar lavado
        /// - CajaViewModel: Al registrar gasto
        /// - EmpleadosViewModel: Al modificar empleados
        /// 
        /// THREADING: El evento se dispara en UI thread (ver NotificarCambios)
        /// </summary>
        public event Action? DatosActualizados;

        // ===================================================================
        // MÉTODO: Notificación segura
        // ===================================================================

        /// <summary>
        /// Notifica a todos los suscriptores que los datos han cambiado.
        /// 
        /// PATRÓN: Thread-Marshalling para UI
        /// 
        /// PROBLEMA:
        /// - Los servicios y ViewModels ejecutan operaciones en background threads
        /// - Los eventos de UI (PropertyChanged, CollectionChanged) deben ejecutarse en UI thread
        /// - Acceder a UI desde background thread causa excepciones o comportamiento indefinido
        /// 
        /// SOLUCIÓN:
        /// MainThread.BeginInvokeOnMainThread() programa la ejecución en el hilo de UI
        /// 
        /// FLUJO:
        /// 1. Servicio guarda datos en background (async/await)
        /// 2. Llama NotificarCambios()
        /// 3. Se programa ejecución en UI thread
        /// 4. UI thread ejecuta DatosActualizados?.Invoke()
        /// 5. Cada suscriptor refresca sus datos en UI thread seguro
        /// 
        /// EJEMPLO DE USO:
        /// await _cajaService.AddGastoAsync(...);
        /// _appStateService.NotificarCambios(); // Home y Caja se actualizan
        /// </summary>
        public void NotificarCambios()
        {
            // BeginInvokeOnMainThread: No bloquea el llamador, programa trabajo asíncrono
            // InvokeOnMainThread: Bloquea esperando ejecución (síncrono)
            // 
            // Usamos BeginInvoke porque no necesitamos esperar resultado,
            // solo garantizar que el evento se dispare en UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // ?.Invoke(): Null-conditional operator, seguro si no hay suscriptores
                // El orden de invocación no está garantizado (multicast delegate)
                DatosActualizados?.Invoke();
            });
        }

        // ===================================================================
        // CONSIDERACIONES DE MEMORIA (Documentación importante)
        // ===================================================================

        // ⚠️ PREVENCIÓN DE MEMORY LEAKS:
        // 
        // Los ViewModels suscritos a este evento deben desuscribirse al destruirse.
        // En MAUI, los ViewModels típicamente viven mientras la página esté en navegación.
        // 
        // PATRÓN CORRECTO EN VIEWMODEL:
        // 
        // public partial class MiViewModel : BaseViewModel
        // {
        //     public MiViewModel(AppStateService appStateService)
        //     {
        //         appStateService.DatosActualizados += OnDatosActualizados;
        //     }
        //     
        //     // En MAUI no hay IDisposable automático para ViewModels,
        //     // pero podemos usar OnDisappearing de la página:
        //     public void Cleanup(AppStateService appStateService)
        //     {
        //         appStateService.DatosActualizados -= OnDatosActualizados;
        //     }
        // }
        // 
        // ALTERNATIVA MODERNA (WeakEventManager en .NET 9):
        // Evita necesidad de desuscripción manual, pero requiere más código.
    }
}