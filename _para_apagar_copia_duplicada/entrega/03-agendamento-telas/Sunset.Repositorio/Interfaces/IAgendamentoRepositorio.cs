using Sunset.Modelos;

namespace Sunset.Repositorio.Interfaces;

public interface IAgendamentoRepositorio
{
    // Executa a stored procedure CadastrarAgendamento e devolve o id criado.
    int CadastrarPelaProcedure(DateTime data, TimeSpan horaInicio, TimeSpan horaFim,
                               int usuarioId, int quadraId, int pagamentoId);

    // Consultas feitas com Entity Framework Core.
    Agendamento? ObterPorId(int id);
    List<Agendamento> ListarPorUsuario(int usuarioId);
    List<Agendamento> ListarDaQuadraNoDia(int quadraId, DateTime dia);
    void Atualizar(Agendamento agendamento);
}
