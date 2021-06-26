/*
A partir de las clases CajaRepository y SucursalRepository, crear la clase BaseRepository<T> 
que unifique los métodos GetAllAsync y GetOneAsync
Crear un abstract BaseEntity que defina la property Id y luego modificar las entities Caja y Sucursal para que hereden de BaseEntity 
Aclaración: Se deben respetar la interfaces. 
*/

namespace Domain.Entities
{
    public abstract class BaseEntity<TId>
    {
        public TId Id { get; }

        protected BaseEntity(TId id) =>
            Id = id;
    }

    public class Caja : BaseEntity<Guid>
    {
        public int SucursalId { get; }
        public string Descripcion { get; }
        public int TipoCajaId { get; }

        public Caja(Guid id, int sucursalId, string descripcion, int tipoCajaId)
            : base(id)
        {
            SucursalId = sucursalId;
            Descripcion = descripcion;
            TipoCajaId = tipoCajaId;
        }
    }

    public class Sucursal : BaseEntity<int>
    {
        public string Direccion { get; }
        public string Telefono { get; }

        public Sucursal(int id, string direccion, string telefono)
            : base(id)
        {
            Direccion = direccion;
            Telefono = telefono;
        }
    }
}

namespace Infrastructure.Data.Repositories
{
    using Domain.Entities;

    public interface ICajaRepository
    {
        Task<IEnumerable<Caja>> GetAllAsync();
        Task<Caja> GetOneAsync(Guid id);
    }

    public interface ISucursalRepository
    {
        Task<IEnumerable<Sucursal>> GetAllAsync();
        Task<Sucursal> GetOneAsync(int id);
    }

    public class BaseRepository<TEntity, TId> where TEntity : BaseEntity<TId>
    {
        protected readonly DataContext _db;

        public BaseRepository(DataContext db) =>
            _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _db.Set<TEntity>().ToListAsync();
        }

        public async Task<TEntity> GetOneAsync(TId id)
        {
            return await _db.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id);
        }
    }

    public class CajaRepository : BaseRepository<Caja, Guid>, ICajaRepository
    {
        public CajaRepository(DataContext db)
            : base(db)
        {
        }
    }

    public class SucursalRepository : BaseRepository<Sucursal, int>, ISucursalRepository
    {
        public CajaRepository(DataContext db)
            : base(db)
        {
        }
    }
}