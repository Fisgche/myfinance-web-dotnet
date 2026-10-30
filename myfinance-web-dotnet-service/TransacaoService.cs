using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet_service
{
    public class TransacaoService : ITransacaoService
    {
        private readonly ITransacaoRepository _transacaoRepository;
        public TransacaoService(ITransacaoRepository transacaoRepository)
        {
            _transacaoRepository = transacaoRepository;
        }

        public void Cadastrar(Transacao entidade)
        {
            _transacaoRepository.Cadastrar(entidade);
        }

        public void Excluir(int id)
        {
            _transacaoRepository.Excluir(id);
        }

        public List<Transacao> ListarRegistros()
        {
            return _transacaoRepository.ListarRegistros();
        }

        public Transacao? RetornarRegistro(int id)
        {
            return _transacaoRepository.RetornarRegistro(id);
        }

        public List<Transacao> ListarPorPeriodo(DateTime inicio, DateTime fim)
        {
            return _transacaoRepository.ListarPorPeriodo(inicio, fim);
        }
    }
}
