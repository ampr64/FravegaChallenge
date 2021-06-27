/*
Teniendo en cuenta la librería ICache, que fue escrita e implementada por otro equipo y utiliza una cache del tipo Key Value,
tomar la clase CajaRepository y modificar los métodos AddAsync, GetAllAsync, GetAllBySucursalAsync y GetOneAsync para que utilicen cache.

Datos:
    * Existen en la empresa 20 sucursales
    * Como mucho hay 100 cajas en la base

Restricción:    
	* Solo es posible utilizar 1 key (IMPORTANTE)
	
Aclaración:
	* No realizar una implementación de ICache, otro equipo la esta brindando
*/

public interface ICache
{
    Task AddAsync<T>(string key, T obj, int? durationInMinutes);
    Task<T> GetOrDefaultAsync<T>(string key);
    Task RemoveAsync(string key);
}

public class CacheConfiguration
{
    public int? CajaCacheDuration { get; set; }
}

public class CajaRepository
{
    private const string _cacheKey = "Caja";
    private readonly DataContext _db;
    private readonly ICache _cache;
    private readonly CacheConfiguration _cacheConfiguration;

    public CajaRepository(DataContext db, ICache cache, IOptions<CacheConfiguration> options)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheConfiguration = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task AddAsync(Caja caja)
    {
        await _db.Cajas.AddAsync(caja);
        await _db.SaveChangesAsync();

        // Remove entry from cache to trigger update
        await _cache.RemoveAsync(_cacheKey);
    }

    public async Task<List<Caja>> GetAllAsync()
    {
        var result = await _cache.GetOrDefaultAsync<List<Caja>>(_cacheKey);

        if (result is null)
        {
            result = await _db.Cajas.AsNoTracking().ToListAsync();
            await _cache.AddAsync<List<Caja>>(_cacheKey, result, _cacheConfiguration.CajaCacheDuration);
        }

        return result;
    }

    public async Task<List<Caja>> GetAllBySucursalAsync(int sucursalId)
    {
        return (await GetAllAsync())
            .Where(c => c.SucursalId == sucursalId)
            .ToList();
    }

    public async Task<Caja> GetOneAsync(Guid id)
    {
        return (await GetAllAsync())
            .FirstOrDefault(c => c.Id == id);
    }
}