namespace CarWashFacil.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private bool _initialized;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task InitAsync()
        {
            if (_initialized && _database != null)
                return;

            await _semaphore.WaitAsync();

            try
            {
                if (_initialized && _database != null)
                    return;

                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "carwashfacil.db3");
                _database = new SQLiteAsyncConnection(dbPath);

                await _database.CreateTableAsync<Usuario>();
                await _database.CreateTableAsync<Empleado>();
                await _database.CreateTableAsync<Lavado>();
                await _database.CreateTableAsync<MovimientoCaja>();
                await _database.CreateTableAsync<EventoSistema>();

                await SeedDataAsync();

                _initialized = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SeedDataAsync()
        {
            if (_database == null) return;

            var admin = await _database.Table<Usuario>()
                .FirstOrDefaultAsync(x => x.Username == "admin");

            if (admin == null)
            {
                await _database.InsertAsync(new Usuario
                {
                    Username = "admin",
                    Password = "1234"
                });
            }

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

        public SQLiteAsyncConnection GetConnection()
        {
            if (_database == null)
                throw new InvalidOperationException("La base de datos no ha sido inicializada.");

            return _database;

        }

    }
}