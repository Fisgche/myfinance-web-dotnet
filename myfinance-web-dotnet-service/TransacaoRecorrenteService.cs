using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces;
using myfinance_web_dotnet_service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myfinance_web_dotnet_service
{
    public class TransacaoRecorrenteService : ITransacaoRecorrenteService
    {
        private readonly ITransacaoRecorrenteRepository _transacaoRecorrenteRepository;
        public TransacaoRecorrenteService(ITransacaoRecorrenteRepository transacaoRecorrenteRepository)
        {
            _transacaoRecorrenteRepository = transacaoRecorrenteRepository;
        }
        public void Cadastrar(TransacaoRecorrente entidade)
        {
            _transacaoRecorrenteRepository.Cadastrar(entidade);
        }

        public void Excluir(int id)
        {
            _transacaoRecorrenteRepository.Excluir(id);
        }

        public List<TransacaoRecorrente> ListarRegistros()
        {
           return _transacaoRecorrenteRepository.ListarRegistros();
        }

        public TransacaoRecorrente RetornarRegistro(int id)
        {
            return _transacaoRecorrenteRepository.RetornarRegistro(id);
        }
    }
}
