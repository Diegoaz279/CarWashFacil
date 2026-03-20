using SQLite;

namespace CarWashFacil.Services
{
    /// <summary>
    /// DATABASE SERVICE: Servicio Singleton de acceso a datos SQLite.
    /// 
    /// PATRÓN: Singleton + Lazy Initialization + Thread-Safe Double-Check Locking
    /// RESPONSABILIDAD: Gestión centralizada de conexión a base de datos SQLite,
    ///                  creación de esquema y datos iniciales (seed).
    /// 
    /// CICLO DE VIDA: 
    /// - Se crea UNA SOLA VEZ al iniciar la aplicación (Singleton en MauiProgram.cs)
    /// - Persiste durante toda la ejecución de la app
    /// - Se destruye cuando la app termina completamente
    /// </summary>
    public class DatabaseService
    {
        // ===================================================================
        // CAMPOS PRIVADOS - Estado interno del servicio
        // ===================================================================

        /// <summary>
        /// Conexión SQLite asíncrona. Nullable porque se inicializa lazy.
        /// </summary>
        private SQLiteAsyncConnection? _database;

        /// <summary>
        /// Bandera de inicialización para evitar múltiples inicializaciones.
        /// volatile asegura visibilidad entre threads (optimización de compilador).
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// SEMAPHORE: Mecanismo de sincronización para thread-safety.
        /// 
        /// PROBLEMA QUE RESUELVE:
        /// - En MAUI, múltiples ViewModels pueden llamar InitAsync() simultáneamente
        /// - Sin sincronización, se crearían múltiples conexiones o race conditions
        /// 
        /// SemaphoreSlim(1, 1) = Solo 1 thread puede entrar a la vez (mutex)
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        // ===================================================================
        // MÉTODO PRINCIPAL: Inicialización Lazy y Thread-Safe
        // ===================================================================

        /// <summary>
        /// Inicializa la base de datos SQLite de forma segura entre threads.
        /// 
        /// PATRÓN: Double-Check Locking (optimizado con SemaphoreSlim)
        /// 
        /// FLUJO:
        /// 1. Verificación rápida (sin lock) - si ya está inicializado, retorna inmediato
        /// 2. Adquiere el semáforo (bloquea otros threads)
        /// 3. Segunda verificación dentro del lock (otro thread pudo inicializar mientras esperaba)
        /// 4. Crea conexión, tablas y datos iniciales
        /// 5. Libera el semáforo (finally garantiza liberación incluso si hay excepción)
        /// 
        /// BENEFICIO: Alto rendimiento en llamadas subsequentes (check rápido sin lock)
        /// </summary>
        public async Task InitAsync()
        {
            // PRIMER CHECK (rápido, sin sincronización): ¿Ya está listo?
            if (_initialized && _database != null)
                return;

            // ADQUIRIR LOCK: Solo un thread puede ejecutar esto a la vez
            await _semaphore.WaitAsync();

            try
            {
                // SEGUNDO CHECK (dentro del lock): ¿Otro thread lo inicializó mientras esperaba?
                if (_initialized && _database != null)
                    return;

                // ------------------------------------------------------------------
                // INICIALIZACIÓN DE BASE DE DATOS
                // ------------------------------------------------------------------

                // FileSystem.AppDataDirectory: Ruta segura para datos de aplicación
                // Android: /data/data/[package]/files/
                // iOS: /Library/
                // Windows: %LocalAppData%/Packages/[package]/LocalState/
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "carwashfacil.db3");

                // Crear conexión SQLite asíncrona (no bloquea UI thread)
                _database = new SQLiteAsyncConnection(dbPath);

                // Crear tablas si no existen (migración automática de esquema)
                await _database.CreateTableAsync<Usuario>();
                await _database.CreateTableAsync<Empleado>();
                await _database.CreateTableAsync<Lavado>();
                await _database.CreateTableAsync<MovimientoCaja>();
                await _database.CreateTableAsync<EventoSistema>();

                // Insertar datos iniciales (solo primera vez)
                await SeedDataAsync();

                // Marcar como inicializado
                _initialized = true;
            }
            finally
            {
                // GARANTÍA: Siempre liberar el semáforo, incluso si hay excepción
                // Evita deadlocks que bloquearían toda la app
                _semaphore.Release();
            }
        }

        // ===================================================================
        // MÉTODO PRIVADO: Datos iniciales (Seed)
        // ===================================================================

        /// <summary>
        /// Inserta datos iniciales si las tablas están vacías.
        /// 
        /// PATRÓN: Database Seeding
        /// 
        /// DATOS CREADOS:
        /// - Usuario admin/1234 para primer acceso
        /// - Empleados de ejemplo (Juan, Pedro)
        /// 
        /// IDEMPOTENCIA: Puede llamarse múltiples veces sin duplicar datos
        /// (verifica existencia antes de insertar)
        /// </summary>
        private async Task SeedDataAsync()
        {
            if (_database == null) return;

            // ------------------------------------------------------------------
            // SEED: Usuario administrador por defecto
            // ------------------------------------------------------------------
            var admin = await _database.Table<Usuario>()
                .FirstOrDefaultAsync(x => x.Username == "admin");

            if (admin == null)
            {
                await _database.InsertAsync(new Usuario
                {
                    Username = "admin",
                    Password = "1234"  // En producción: hash con BCrypt/Argon2
                });
            }

            // ------------------------------------------------------------------
            // SEED: Empleados de ejemplo
            // ------------------------------------------------------------------
            var empleados = await _database.Table<Empleado>().ToListAsync();
            if (empleados.Count == 0)
            {
                await _database.InsertAllAsync(new List<Empleado>
                {
                    new Empleado { Nombre = "Juan", Telefono = "809-000-0001", Activo = true },
                    new Empleado { Nombre = "Pedro", Telefono = "809-000-0002", Activo = true }
                });
            }
        }

        // ===================================================================
        // MÉTODO DE ACCESO: Obtener conexión
        // ===================================================================

        /// <summary>
        /// Proporciona acceso a la conexión SQLite inicializada.
        /// 
        /// PRECONDICIÓN: Debe llamarse InitAsync() antes (implícito en servicios)
        /// 
        /// EXCEPCIÓN: InvalidOperationException si no está inicializado
        /// Esto es intencional: falla rápido en lugar de comportamiento silencioso/buggy
        /// 
        /// USO TÍPICO EN SERVICIOS:
        /// await _databaseService.InitAsync();
        /// var db = _databaseService.GetConnection();
        /// </summary>
        public SQLiteAsyncConnection GetConnection()
        {
            if (_database == null)
                throw new InvalidOperationException(
                    "La base de datos no ha sido inicializada. " +
                    "Llame InitAsync() antes de obtener la conexión.");

            return _database;
        }
    }
}