namespace Sunset.Repositorio.Interfaces;

public interface IAgendamentoRepositorio
{
    // Executa a stored procedure CadastrarAgendamento e devolve o id criado.
    int CadastrarPelaProcedure(DateTime data, TimeSpan horaInicio, TimeSpan horaFim,
                               int usuarioId, int quadraId, int pagamentoId);
}
