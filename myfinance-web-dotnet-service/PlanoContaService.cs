using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet_service
{
    public class PlanoContaService : IPlanoContaService
    {
        private readonly IPlanoContaRepository _planoContaRepository;
        public PlanoContaService(IPlanoContaRepository planoContaRepository)
        {
            _planoContaRepository = planoContaRepository;
        }
        public void Cadastrar(PlanoConta entidade)
        {
            _planoContaRepository.Cadastrar(entidade);
        }

        public void Excluir(int id)
        {
            _planoContaRepository.Excluir(id);
        }

        public List<PlanoConta> ListarRegistros()
        {
            return _planoContaRepository.ListarRegistros();
        }

        public PlanoConta RetornarRegistro(int id)
        {
            return _planoContaRepository.RetornarRegistro(id);
        }
    }
}