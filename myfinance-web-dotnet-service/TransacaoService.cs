using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet_service
{
    public class TransacaoService : ITransacaoService
    {
        private readonly MyFinanceDbContext _dbContext;
        public TransacaoService(MyFinanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Cadastrar(Transacao entidade)
        {
            var dbSet = _dbContext.Transacao;

            if(entidade.Id == null)
            {
                dbSet.Add(entidade);
            }
            else
            {
                dbSet.Attach(entidade);
                _dbContext.Entry(entidade).State = EntityState.Modified; 
            }

            _dbContext.SaveChanges();
        }

        public void Excluir(int id)
        {
            var Transacao = new Transacao(){ Id = id };
            _dbContext.Attach(Transacao);
            _dbContext.Remove(Transacao);
            _dbContext.SaveChanges();

        }

        public List<Transacao> ListarRegistros()
        {
            var dbSet = _dbContext.Transacao;
            return dbSet.ToList();
        }

        public Transacao RetornarRegistro(int id)
        {
            return _dbContext.Transacao.Where(x=> x.Id == id).First();
        }
    }
}