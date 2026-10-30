using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_domain.Enums;
using myfinance_web_dotnet_infra.Interfaces;
using myfinance_web_dotnet_service.Dtos;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet_service
{
    public class DashboardService : IDashboardService
    {
        private readonly ITransacaoRepository _transacaoRepository;

        private readonly ITransacaoRecorrenteRepository _transacaoRecorrenteRepository;

        public DashboardService(ITransacaoRepository transacaoRepository, ITransacaoRecorrenteRepository transacaoRecorrenteRepository)
        {
            _transacaoRepository = transacaoRepository;
            _transacaoRecorrenteRepository = transacaoRecorrenteRepository;
        }

        public ResumoMensal ObterResumoMensal(int ano, int mes)
        {
            var inicio = new DateTime(ano, mes, 1);
            var fim = inicio.AddMonths(1);

            var lancamentos = new List<LancamentoMensal>();

            foreach(var transacao in _transacaoRepository.ListarPorPeriodo(inicio, fim))
            {
                lancamentos.Add(new LancamentoMensal()
                {
                    Id = transacao.Id,
                    Data = transacao.Date.Date,
                    Descricao = transacao.Historico,
                    PlanoContaId = transacao.PlanoContaId,
                    Categoria = transacao.PlanoConta?.Descricao ?? string.Empty,
                    Tipo = ObterTipo(transacao.PlanoConta),
                    Valor = transacao.Valor
                });
            }

            foreach(var transacaoRecorrente in _transacaoRecorrenteRepository.ListarVigentesNoPeriodo(inicio, fim))
            {
                var ocorrencia = CalcularOcorrencia(transacaoRecorrente, ano, mes);
                if(ocorrencia == null)
                {
                    continue;
                }

                lancamentos.Add(new LancamentoMensal()
                {
                    Id = transacaoRecorrente.Id,
                    Recorrente = true,
                    Data = ocorrencia.Value,
                    Descricao = $"Recorrente ({transacaoRecorrente.Frequencia.ObterDescricao().ToLowerInvariant()})",
                    PlanoContaId = transacaoRecorrente.PlanoContaId,
                    Categoria = transacaoRecorrente.PlanoConta?.Descricao ?? string.Empty,
                    Tipo = ObterTipo(transacaoRecorrente.PlanoConta),
                    Valor = transacaoRecorrente.Valor
                });
            }

            lancamentos = lancamentos
                .OrderBy(x => x.Data)
                .ThenBy(x => x.Recorrente)
                .ThenBy(x => x.Id)
                .ToList();

            var resumo = new ResumoMensal()
            {
                Ano = ano,
                Mes = mes,
                Lancamentos = lancamentos,
                TotalReceitas = Somar(lancamentos, TipoLancamento.Receita),
                TotalDespesas = Somar(lancamentos, TipoLancamento.Despesa),
                TotalInvestimentos = Somar(lancamentos, TipoLancamento.Investimento)
            };

            resumo.DespesasPorCategoria = AgruparPorCategoria(lancamentos, TipoLancamento.Despesa, resumo.TotalDespesas);
            resumo.EvolucaoDiaria = CalcularEvolucaoDiaria(lancamentos, ano, mes);

            return resumo;
        }

        /// <summary>
        /// Data em que a transacao recorrente ocorre no mes informado, ou null se nao ocorrer.
        /// As ocorrencias sao DataInicio + k * Frequencia meses (k = 0, 1, 2...) ate DataFim.
        /// Em meses mais curtos o dia e ajustado para o ultimo dia do mes (ex.: 31/01 -> 28/02).
        /// </summary>
        public static DateTime? CalcularOcorrencia(TransacaoRecorrente transacaoRecorrente, int ano, int mes)
        {
            var dataInicio = transacaoRecorrente.DataInicio.Date;
            var mesesDesdeInicio = (ano - dataInicio.Year) * 12 + (mes - dataInicio.Month);
            var intervaloEmMeses = Math.Max(1, (int)transacaoRecorrente.Frequencia);

            if(mesesDesdeInicio < 0 || mesesDesdeInicio % intervaloEmMeses != 0)
            {
                return null;
            }

            // Sempre a partir de DataInicio (e nao da ocorrencia anterior) para o dia nao "encolher" apos fevereiro
            var ocorrencia = dataInicio.AddMonths(mesesDesdeInicio);

            return ocorrencia <= transacaoRecorrente.DataFim.Date ? ocorrencia : null;
        }

        private static TipoLancamento ObterTipo(PlanoConta? planoConta)
        {
            return planoConta?.Tipo?.Trim().ToUpperInvariant() switch
            {
                "R" => TipoLancamento.Receita,
                "D" => TipoLancamento.Despesa,
                "I" => TipoLancamento.Investimento,
                _ => TipoLancamento.Indefinido
            };
        }

        private static decimal Somar(IEnumerable<LancamentoMensal> lancamentos, TipoLancamento tipo)
        {
            return lancamentos.Where(x => x.Tipo == tipo).Sum(x => x.Valor);
        }

        private static List<TotalPorCategoria> AgruparPorCategoria(List<LancamentoMensal> lancamentos, TipoLancamento tipo, decimal total)
        {
            return lancamentos
                .Where(x => x.Tipo == tipo)
                .GroupBy(x => new { x.PlanoContaId, x.Categoria })
                .Select(grupo => new TotalPorCategoria()
                {
                    PlanoContaId = grupo.Key.PlanoContaId,
                    Categoria = grupo.Key.Categoria,
                    Valor = grupo.Sum(x => x.Valor),
                    Percentual = total == 0 ? 0 : Math.Round(grupo.Sum(x => x.Valor) / total * 100, 1)
                })
                .OrderByDescending(x => x.Valor)
                .ThenBy(x => x.Categoria)
                .ToList();
        }

        private static List<AcumuladoDiario> CalcularEvolucaoDiaria(List<LancamentoMensal> lancamentos, int ano, int mes)
        {
            var evolucao = new List<AcumuladoDiario>();
            var lancamentosPorDia = lancamentos.ToLookup(x => x.Data.Day);
            decimal receitas = 0, despesas = 0, investimentos = 0;

            for(var dia = 1; dia <= DateTime.DaysInMonth(ano, mes); dia++)
            {
                receitas += Somar(lancamentosPorDia[dia], TipoLancamento.Receita);
                despesas += Somar(lancamentosPorDia[dia], TipoLancamento.Despesa);
                investimentos += Somar(lancamentosPorDia[dia], TipoLancamento.Investimento);

                evolucao.Add(new AcumuladoDiario()
                {
                    Data = new DateTime(ano, mes, dia),
                    Receitas = receitas,
                    Despesas = despesas,
                    Investimentos = investimentos
                });
            }

            return evolucao;
        }
    }
}
