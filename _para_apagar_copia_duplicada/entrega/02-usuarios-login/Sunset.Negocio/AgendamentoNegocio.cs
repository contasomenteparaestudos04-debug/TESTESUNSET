using Sunset.Repositorio;
using Sunset.Repositorio.Interfaces;

namespace Sunset.Negocio;

public class AgendamentoNegocio
{
    private readonly IAgendamentoRepositorio _repositorio;

    public AgendamentoNegocio(IAgendamentoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // Cria o agendamento pela stored procedure. O conflito de horário
    // é conferido dentro da procedure, no banco.
    public int Agendar(DateTime data, TimeSpan horaInicio, TimeSpan horaFim,
                       int usuarioId, int quadraId, int pagamentoId)
    {
        if (horaFim <= horaInicio)
            throw new RegraDeNegocioException("O horário de término deve ser maior que o horário de início.");

        try
        {
            return _repositorio.CadastrarPelaProcedure(data, horaInicio, horaFim,
                                                       usuarioId, quadraId, pagamentoId);
        }
        catch (ErroDeBancoException ex)
        {
            // Mensagem da procedure (conflito, vínculo inexistente) vira erro de regra.
            throw new RegraDeNegocioException(ex.Message);
        }
    }
}
