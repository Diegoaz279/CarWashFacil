namespace CarWashFacil.Services
{
    // ═════════════════════════════════════════════════════════════════════════════
    // SERVICIO DE AUTENTICACIÓN
    // Responsabilidad: Validar credenciales de usuario
    // NOTA DE SEGURIDAD: Las contraseñas se comparan en texto plano. 
    // En producción, implementar hashing (BCrypt/SHA256 con salt)
    // ═════════════════════════════════════════════════════════════════════════════
    public class AuthService
    {
        // Dependencia inyectada - patrón Dependency Injection
        private readonly DatabaseService _databaseService;

        // Constructor que recibe el servicio de base de datos
        // Esto permite testing y desacoplamiento
        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Valida credenciales de usuario contra la base de datos local
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="password">Contraseña en texto plano (⚠️ vulnerable)</param>
        /// <returns>True si las credenciales son válidas, False si no</returns>
        public async Task<bool> LoginAsync(string username, string password)
        {
            // Inicializa la conexión a SQLite si no está lista
            await _databaseService.InitAsync();

            // Obtiene conexión activa a la base de datos
            var db = _databaseService.GetConnection();

            // ⚠️ VULNERABILIDAD: Consulta directa con texto plano
            // En producción: comparar hash de la contraseña ingresada
            // contra el hash almacenado en la base de datos
            var user = await db.Table<Usuario>()
                .FirstOrDefaultAsync(x => x.Username == username && x.Password == password);

            // Retorna true si encontró coincidencia, false si no
            return user != null;
        }
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // SERVICIO DE GESTIÓN DE EMPLEADOS
    // Responsabilidad: CRUD completo de empleados (Create, Read, Update, Delete)
    // Incluye filtrado por estado activo/inactivo
    // ═════════════════════════════════════════════════════════════════════════════
    public class EmpleadoService
    {
        private readonly DatabaseService _databaseService;

        public EmpleadoService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Obtiene TODOS los empleados ordenados alfabéticamente por nombre
        /// </summary>
        /// <returns>Lista de todos los empleados (activos e inactivos)</returns>
        public async Task<List<Empleado>> GetAllAsync()
        {
            await _databaseService.InitAsync();
            
            return await _databaseService.GetConnection().Table<Empleado>()
                .OrderBy(x => x.Nombre)  // Ordenamiento alfabético A-Z
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene solo empleados activos (útil para asignar lavados)
        /// </summary>
        /// <returns>Lista de empleados donde Activo == true</returns>
        public async Task<List<Empleado>> GetActivosAsync()
        {
            await _databaseService.InitAsync();
            
            return await _databaseService.GetConnection().Table<Empleado>()
                .Where(x => x.Activo)    // Filtro: solo activos
                .OrderBy(x => x.Nombre)  // Ordenamiento alfabético
                .ToListAsync();
        }

        /// <summary>
        /// Inserta un nuevo empleado en la base de datos
        /// </summary>
        /// <param name="empleado">Objeto Empleado a insertar</param>
        /// <exception cref="Exception">Relanza cualquier error de base de datos</exception>
        public async Task AddAsync(Empleado empleado)
        {
            try
            {
                await _databaseService.InitAsync();
                
                // InsertAsync genera automáticamente el ID si es autoincremental
                await _databaseService.GetConnection().InsertAsync(empleado);
            }
            catch (Exception ex)
            {
                // Logging básico a consola de debug
                // TODO: Implementar ILogger<T> para producción
                System.Diagnostics.Debug.WriteLine($"Error insertando empleado: {ex}");
                throw; // Relanza para que la UI maneje el error
            }
        }

        /// <summary>
        /// Actualiza datos de un empleado existente
        /// </summary>
        /// <param name="empleado">Objeto Empleado con datos actualizados (requiere ID)</param>
        public async Task UpdateAsync(Empleado empleado)
        {
            try
            {
                await _databaseService.InitAsync();
                await _databaseService.GetConnection().UpdateAsync(empleado);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando empleado: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Elimina permanentemente un empleado de la base de datos
        /// ⚠️ CONSIDERAR: Implementar soft-delete (marcar inactivo) en lugar de eliminar físicamente
        /// para mantener integridad referencial con lavados históricos
        /// </summary>
        /// <param name="empleado">Objeto Empleado a eliminar (requiere ID)</param>
        public async Task DeleteAsync(Empleado empleado)
        {
            try
            {
                await _databaseService.InitAsync();
                await _databaseService.GetConnection().DeleteAsync(empleado);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error eliminando empleado: {ex}");
                throw;
            }
        }
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // SERVICIO DE GESTIÓN DE LAVADOS
    // Responsabilidad: Registrar y consultar servicios de lavado de autos
    // Incluye estadísticas de operación (totales diarios/mensuales)
    // ═════════════════════════════════════════════════════════════════════════════
    public class LavadoService
    {
        private readonly DatabaseService _databaseService;

        public LavadoService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Obtiene todos los lavados ordenados por fecha descendente (más recientes primero)
        /// </summary>
        /// <returns>Lista completa de lavados registrados</returns>
        public async Task<List<Lavado>> GetAllAsync()
        {
            await _databaseService.InitAsync();
            
            return await _databaseService.GetConnection().Table<Lavado>()
                .OrderByDescending(x => x.Fecha)  // Más recientes primero
                .ToListAsync();
        }

        /// <summary>
        /// Registra un nuevo lavado en el sistema
        /// </summary>
        /// <param name="lavado">Objeto Lavado con datos del servicio</param>
        public async Task AddAsync(Lavado lavado)
        {
            try
            {
                await _databaseService.InitAsync();
                await _databaseService.GetConnection().InsertAsync(lavado);
                
                // TODO: Considerar transacción atómica:
                // 1. Insertar lavado
                // 2. Registrar ingreso automático en CajaService
                // para mantener consistencia entre inventario y finanzas
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al insertar lavado: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Actualiza información de un lavado existente
        /// Útil para cambiar estado (Pendiente -> En Proceso -> Completado/Cancelado)
        /// </summary>
        /// <param name="lavado">Objeto Lavado actualizado</param>
        public async Task UpdateAsync(Lavado lavado)
        {
            try
            {
                await _databaseService.InitAsync();
                await _databaseService.GetConnection().UpdateAsync(lavado);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar lavado: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Calcula total de lavados realizados hoy (excluye cancelados)
        /// ⚠️ INEFICIENTE: Carga todos los registros a memoria y filtra en C#
        /// Optimización sugerida: consulta SQL directa con COUNT
        /// </summary>
        /// <returns>Cantidad de lavados no cancelados del día actual</returns>
        public async Task<int> TotalLavadosHoyAsync()
        {
            // ⚠️ PROBLEMA DE RENDIMIENTO: GetAllAsync() trae TODOS los lavados a memoria
            var data = await GetAllAsync();
            
            // Filtrado en memoria (ineficiente con grandes volúmenes de datos)
            return data.Count(x => x.Fecha.Date == DateTime.Today && x.Estado != "Cancelado");
            
            /* OPTIMIZACIÓN RECOMENDADA:
            return await _databaseService.GetConnection().Table<Lavado>()
                .CountAsync(x => x.Fecha.Date == DateTime.Today && x.Estado != "Cancelado");
            */
        }

        /// <summary>
        /// Calcula total de lavados del mes actual (excluye cancelados)
        /// ⚠️ Mismo problema de rendimiento que TotalLavadosHoyAsync
        /// </summary>
        /// <returns>Cantidad de lavados no cancelados del mes en curso</returns>
        public async Task<int> TotalLavadosMesAsync()
        {
            var now = DateTime.Now;
            var data = await GetAllAsync();
            
            return data.Count(x => 
                x.Fecha.Month == now.Month && 
                x.Fecha.Year == now.Year && 
                x.Estado != "Cancelado");
        }
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // SERVICIO DE CONTROL DE CAJA (FINANZAS)
    // Responsabilidad: Registrar movimientos de ingresos y gastos
    // Calcula balances diarios y mensuales
    // ═════════════════════════════════════════════════════════════════════════════
    public class CajaService
    {
        private readonly DatabaseService _databaseService;

        public CajaService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Obtiene todos los movimientos de caja ordenados por fecha descendente
        /// </summary>
        /// <returns>Lista de ingresos y gastos</returns>
        public async Task<List<MovimientoCaja>> GetAllAsync()
        {
            await _databaseService.InitAsync();
            
            return await _databaseService.GetConnection().Table<MovimientoCaja>()
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();
        }

        /// <summary>
        /// Registra un ingreso en caja (venta, pago de servicio, etc.)
        /// </summary>
        /// <param name="descripcion">Concepto del ingreso</param>
        /// <param name="monto">Cantidad positiva recibida</param>
        public async Task AddIngresoAsync(string descripcion, decimal monto)
        {
            try
            {
                await _databaseService.InitAsync();

                // Crea objeto MovimientoCaja con tipo "Ingreso"
                // TODO: Validar que monto > 0
                await _databaseService.GetConnection().InsertAsync(new MovimientoCaja
                {
                    Tipo = "Ingreso",        // ⚠️ Hardcoded string - usar enum
                    Descripcion = descripcion,
                    Monto = monto,
                    Fecha = DateTime.Now     // Fecha/hora del servidor local
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando ingreso: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Registra un gasto/egreso en caja (compras, gastos operativos, etc.)
        /// </summary>
        /// <param name="descripcion">Concepto del gasto</param>
        /// <param name="monto">Cantidad positiva gastada (se restará del balance)</param>
        public async Task AddGastoAsync(string descripcion, decimal monto)
        {
            try
            {
                await _databaseService.InitAsync();

                // Crea objeto MovimientoCaja con tipo "Gasto"
                // TODO: Validar que monto > 0
                await _databaseService.GetConnection().InsertAsync(new MovimientoCaja
                {
                    Tipo = "Gasto",           // ⚠️ Hardcoded string - usar enum
                    Descripcion = descripcion,
                    Monto = monto,
                    Fecha = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando gasto: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Suma todos los ingresos registrados hoy
        /// ⚠️ INEFICIENTE: Carga todos los movimientos a memoria
        /// </summary>
        /// <returns>Total de ingresos del día actual</returns>
        public async Task<decimal> TotalIngresosHoyAsync()
        {
            var data = await GetAllAsync();
            
            return data
                .Where(x => x.Tipo == "Ingreso" && x.Fecha.Date == DateTime.Today)
                .Sum(x => x.Monto);
        }

        /// <summary>
        /// Suma todos los gastos registrados hoy
        /// </summary>
        /// <returns>Total de gastos del día actual</returns>
        public async Task<decimal> TotalGastosHoyAsync()
        {
            var data = await GetAllAsync();
            
            return data
                .Where(x => x.Tipo == "Gasto" && x.Fecha.Date == DateTime.Today)
                .Sum(x => x.Monto);
        }

        /// <summary>
        /// Calcula balance del día (Ingresos - Gastos)
        /// </summary>
        /// <returns>Ganancia neta del día (puede ser negativa)</returns>
        public async Task<decimal> BalanceHoyAsync()
        {
            return await TotalIngresosHoyAsync() - await TotalGastosHoyAsync();
        }

        /// <summary>
        /// Suma todos los ingresos del mes en curso
        /// </summary>
        /// <returns>Total de ingresos del mes actual</returns>
        public async Task<decimal> TotalIngresosMesAsync()
        {
            var now = DateTime.Now;
            var data = await GetAllAsync();
            
            return data
                .Where(x => x.Tipo == "Ingreso" && 
                       x.Fecha.Month == now.Month && 
                       x.Fecha.Year == now.Year)
                .Sum(x => x.Monto);
        }

        /// <summary>
        /// Suma todos los gastos del mes en curso
        /// </summary>
        /// <returns>Total de gastos del mes actual</returns>
        public async Task<decimal> TotalGastosMesAsync()
        {
            var now = DateTime.Now;
            var data = await GetAllAsync();
            
            return data
                .Where(x => x.Tipo == "Gasto" && 
                       x.Fecha.Month == now.Month && 
                       x.Fecha.Year == now.Year)
                .Sum(x => x.Monto);
        }

        /// <summary>
        /// Calcula balance mensual (Ingresos - Gastos)
        /// </summary>
        /// <returns>Ganancia neta del mes (puede ser negativa)</returns>
        public async Task<decimal> BalanceMesAsync()
        {
            return await TotalIngresosMesAsync() - await TotalGastosMesAsync();
        }
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // SERVICIO DE CICLO DE VIDA 
    // Responsabilidad: Registrar eventos del sistema para debugging y trazabilidad
    // ═════════════════════════════════════════════════════════════════════════════
    public class LifecycleService
    {
        private readonly DatabaseService _databaseService;

        public LifecycleService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        
        /// Registra un evento de sistema con clasificación automática por tipo.
        /// 
        /// CLASIFICACIÓN DE EVENTOS (para visualización colorida en UI):
        /// • Exito (verde): OnStart, OnResume, Activated, eventos positivos
        /// • Advertencia (ámbar): OnSleep, Deactivated, pausas
        /// • Info (azul): OnAppearing, OnDisappearing, eventos de página
        /// • Window (púrpura): Window events (Stopped, Resumed)
        /// • Error (rojo): Excepciones, fallos
        
        public async Task AddEventAsync(string mensaje)
        {
            await _databaseService.InitAsync();

            // ⭐ NUEVO: Clasificación automática por tipo
            var tipo = ClasificarTipoEvento(mensaje);

            await _databaseService.GetConnection().InsertAsync(new EventoSistema
            {
                Mensaje = mensaje,
                Tipo = tipo,  // ⭐ NUEVO: Guardar el tipo para colores
                Fecha = DateTime.Now
            });
        }

        /// <summary>
        /// Versión "segura" que no lanza excepciones (fire-and-forget)
        /// </summary>
        public Task AddEventSafeAsync(string mensaje)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await AddEventAsync(mensaje);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error guardando evento: {ex}");
                }
            });
        }

        // ⭐ NUEVO: Método para clasificar eventos por tipo
        private string ClasificarTipoEvento(string mensaje)
        {
            var msgLower = mensaje.ToLower();

            // 🟢 ÉXITO: Start, Resume, Activated, creada, reanudada, abierta
            if (msgLower.Contains("start") ||
                msgLower.Contains("resume") ||
                (msgLower.Contains("activ") && !msgLower.Contains("deactiv")) ||
                msgLower.Contains("creada") ||
                msgLower.Contains("reanudada") ||
                msgLower.Contains("abierta"))
                return "Exito";

            // 🟡 ADVERTENCIA: Sleep, Deactivated, cerrada, detenida, background
            if (msgLower.Contains("sleep") ||
                msgLower.Contains("deactiv") ||
                msgLower.Contains("cerrada") ||
                msgLower.Contains("detenida") ||
                msgLower.Contains("background"))
                return "Advertencia";

            // 🟣 WINDOW: Eventos específicos de ventana
            if (msgLower.Contains("window") ||
                msgLower.Contains("stopped") ||
                msgLower.Contains("resumed"))
                return "Window";

            // 🔵 INFO (default): Página, appearing, disappearing
            if (msgLower.Contains("página") ||
                msgLower.Contains("page"))
                return "Info";

            // 🔴 ERROR: Excepciones, fallos
            if (msgLower.Contains("error") ||
                msgLower.Contains("excepción") ||
                msgLower.Contains("fallo"))
                return "Error";

            // Por defecto: Info
            return "Info";
        }

        public async Task<List<EventoSistema>> GetEventsAsync()
        {
            await _databaseService.InitAsync();

            return await _databaseService.GetConnection().Table<EventoSistema>()
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();
        }

        public async Task ClearAsync()
        {
            await _databaseService.InitAsync();
            await _databaseService.GetConnection().DeleteAllAsync<EventoSistema>();
        }
    }
}