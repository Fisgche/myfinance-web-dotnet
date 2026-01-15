using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using myfinance_web_dotnet_domain.Entities.Base;
using myfinance_web_dotnet_infra.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var entidade = new T(){ Id = id };
            Db.Attach(entidade);
            Db.Remove(entidade);
            Db.SaveChanges();
        }

        public List<T> ListarRegistros()
        {
            return DbSetContext.ToList();
        }

        public T RetornarRegistro(int id)
        {
            return DbSetContext.Where(x=> x.Id == id).First();
        }
    }
}
