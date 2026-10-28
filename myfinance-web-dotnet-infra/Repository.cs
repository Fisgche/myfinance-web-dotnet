using Microsoft.EntityFrameworkCore;
using myfinance_web_dotnet_domain.Entities.Base;
using myfinance_web_dotnet_infra.Interfaces.Base;

namespace myfinance_web_dotnet_infra
{
    public abstract class Repository<T> : IRepository<T> where T : EntityBase, new()
    {
        protected DbContext Db;
        protected DbSet<T> DbSetContext;
        protected Repository(DbContext dbContext)
        {
            Db = dbContext;
            DbSetContext = Db.Set<T>();
        }

        public void Cadastrar(T entidade)
        {
            if(entidade.Id == null)
            {
                DbSetContext.Add(entidade);
            }
            else
            {
                DbSetContext.Attach(entidade);
                Db.Entry(entidade).State = EntityState.Modified; 
            }

            Db.SaveChanges();
        }

        public void Excluir(int id)
        {
            var entidade = DbSetContext.Find(id);
            if(entidade == null)
            {
                return;
            }

            DbSetContext.Remove(entidade);
            Db.SaveChanges();
        }

        public virtual List<T> ListarRegistros()
        {
            return DbSetContext.ToList();
        }

        public T? RetornarRegistro(int id)
        {
            return DbSetContext.FirstOrDefault(x=> x.Id == id);
        }
    }
}
