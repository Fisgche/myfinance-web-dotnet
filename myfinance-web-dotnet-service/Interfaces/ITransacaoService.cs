using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myfinance_web_dotnet_domain.Entities;

namespace myfinance_web_dotnet_service.Interfaces
{
    public interface ITransacaoService
    {
        void Cadastrar(Transacao entidade);

        void Excluir(int id);

        List<Transacao> ListarRegistros();

        Transacao RetornarRegistro(int id);
    }
}