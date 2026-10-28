namespace myfinance_web_dotnet_infra.Interfaces.Base
{
    public interface IRepository<T> where T : class
    {
        void Cadastrar(T entidade);

        void Excluir(int id);

        List<T> ListarRegistros();

        T? RetornarRegistro(int id);
    }
}
