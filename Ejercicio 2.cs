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

using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface ICache
{
    Task AddAsync<T>(string key, T obj, int? durationInMinutes);
    Task<T> GetOrDefaultAsync<T>(string key);
    Task RemoveAsync(string key);
}

public class DataContext : DbContext
{
    public DbSet<Caja> Cajas { get; set; }
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
        _db = db ?? throw new ArgumentNullException(nameof(DataContext));
        _cache = cache ?? throw new ArgumentNullException(nameof(ICache));
        _cacheConfiguration = options.Value ?? throw new ArgumentNullException(nameof(CacheConfiguration));
    }

    public async Task AddAsync(Caja caja)
    {
        await _db.Cajas.AddAsync(caja);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Caja>> GetAllAsync()
    {
        var cacheCajas = await _cache.GetOrDefaultAsync<List<Caja>>(_cacheKey);

        if (cacheCajas != null)
        {
            return cacheCajas;
        }

        cacheCajas = await _db.Cajas.ToListAsync();

        await _cache.AddAsync<List<Caja>>(_cacheKey, cacheCajas, _cacheConfiguration.CajaCacheDuration);

        return cacheCajas;
    }

    public async Task<List<Caja>> GetAllBySucursalAsync(int sucursalId)
    {
        var cacheCajas = await _cache.GetOrDefaultAsync<List<Caja>>(_cacheKey);

        if (cacheCajas != null)
        {
            return cacheCajas.Where(c => c.SucursalId == sucursalId);
        }

        return await _db.Cajas.Where(c => c.SucursalId == sucursalId)
            .ToListAsync();
    }

    public async Task<Caja> GetOneAsync(Guid id)
    {
        var cacheCajas = await _cache.GetOrDefaultAsync<List<Caja>>(_cacheKey);
        var caja = null;

        if (cacheCajas != null)
        {
            caja = cacheCajas.FirstOrDefault(c => c.Id == id);
        }

        return caja ?? await _db.Cajas.FirstOrDefaultAsync(c => c.Id == id);
    }
}