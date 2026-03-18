namespace CarWashFacil.Services
{
    public class AuthService
    {
        private readonly DatabaseService _databaseService;

        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            await _databaseService.InitAsync();

            var db = _databaseService.GetConnection();

            var user = await db.Table<Usuario>()
                .FirstOrDefaultAsync(x => x.Username == username && x.Password == password);

            return user != null;
        }
    }

    public class EmpleadoService
    {
        private readonly DatabaseService _databaseService;

        public EmpleadoService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Empleado>> GetAllAsync()
        {
            await _databaseService.InitAsync();
            return await _databaseService.GetConnection().Table<Empleado>()
                .OrderBy(x => x.Nombre)
                .ToListAsync();
        }

        public async Task<List<Empleado>> GetActivosAsync()
        {
            await _databaseService.InitAsync();
            return await _databaseService.GetConnection().Table<Empleado>()
                .Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .ToListAsync();
        }

        public async Task AddAsync(Empleado empleado)
        {
            try
            {
                await _databaseService.InitAsync();
                await _databaseService.GetConnection().InsertAsync(empleado);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error insertando empleado: {ex}");
                throw;
            }
        }

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

    public class LavadoService
    {
        private readonly DatabaseService _databaseService;

        public LavadoService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Lavado>> GetAllAsync()
        {
            await _databaseService.InitAsync();
            return await _databaseService.GetConnection().Table<Lavado>()
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();
        }

        public async Task AddAsync(Lavado lavado)
        {
            try
            {
                await _databaseService.InitAsync();
                await _databaseService.GetConnection().InsertAsync(lavado);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al insertar lavado: {ex}");
                throw;
            }
        }

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

        public async Task<int> TotalLavadosHoyAsync()
        {
            var data = await GetAllAsync();
            return data.Count(x => x.Fecha.Date == DateTime.Today && x.Estado != "Cancelado");
        }

        public async Task<int> TotalLavadosMesAsync()
        {
            var now = DateTime.Now;
            var data = await GetAllAsync();
            return data.Count(x => x.Fecha.Month == now.Month && x.Fecha.Year == now.Year && x.Estado != "Cancelado");
        }
    }

    public class CajaService
    {
        private readonly DatabaseService _databaseService;

        public CajaService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<MovimientoCaja>> GetAllAsync()
        {
            await _databaseService.InitAsync();
            return await _databaseService.GetConnection().Table<MovimientoCaja>()
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();
        }

        public async Task AddIngresoAsync(string descripcion, decimal monto)
        {
            try
            {
                await _databaseService.InitAsync();

                await _databaseService.GetConnection().InsertAsync(new MovimientoCaja
                {
                    Tipo = "Ingreso",
                    Descripcion = descripcion,
                    Monto = monto,
                    Fecha = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando ingreso: {ex}");
                throw;
            }
        }

        public async Task AddGastoAsync(string descripcion, decimal monto)
        {
            try
            {
                await _databaseService.InitAsync();

                await _databaseService.GetConnection().InsertAsync(new MovimientoCaja
                {
                    Tipo = "Gasto",
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

        public async Task<decimal> TotalIngresosHoyAsync()
        {
            var data = await GetAllAsync();
            return data.Where(x => x.Tipo == "Ingreso" && x.Fecha.Date == DateTime.Today)
                       .Sum(x => x.Monto);
        }

        public async Task<decimal> TotalGastosHoyAsync()
        {
            var data = await GetAllAsync();
            return data.Where(x => x.Tipo == "Gasto" && x.Fecha.Date == DateTime.Today)
                       .Sum(x => x.Monto);
        }

        public async Task<decimal> BalanceHoyAsync()
        {
            return await TotalIngresosHoyAsync() - await TotalGastosHoyAsync();
        }

        public async Task<decimal> TotalIngresosMesAsync()
        {
            var now = DateTime.Now;
            var data = await GetAllAsync();
            return data.Where(x => x.Tipo == "Ingreso" && x.Fecha.Month == now.Month && x.Fecha.Year == now.Year)
                       .Sum(x => x.Monto);
        }

        public async Task<decimal> TotalGastosMesAsync()
        {
            var now = DateTime.Now;
            var data = await GetAllAsync();
            return data.Where(x => x.Tipo == "Gasto" && x.Fecha.Month == now.Month && x.Fecha.Year == now.Year)
                       .Sum(x => x.Monto);
        }

        public async Task<decimal> BalanceMesAsync()
        {
            return await TotalIngresosMesAsync() - await TotalGastosMesAsync();
        }
    }

    public class LifecycleService
    {
        private readonly DatabaseService _databaseService;

        public LifecycleService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task AddEventAsync(string mensaje)
        {
            await _databaseService.InitAsync();

            await _databaseService.GetConnection().InsertAsync(new EventoSistema
            {
                Mensaje = mensaje,
                Fecha = DateTime.Now
            });
        }

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