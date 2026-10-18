using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sunset.Repositorio.Contexto;
using Sunset.Repositorio.Interfaces;

namespace Sunset.Repositorio;

public class AgendamentoRepositorio : IAgendamentoRepositorio
{
    private readonly AppDbContext _contexto;

    public AgendamentoRepositorio(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    // ---------- Dapper + stored procedure ----------
    public int CadastrarPelaProcedure(DateTime data, TimeSpan horaInicio, TimeSpan horaFim,
                                      int usuarioId, int quadraId, int pagamentoId)
    {
        // Reaproveita a mesma conexão (e a mesma connection string) do EF.
        var conexao = _contexto.Database.GetDbConnection();

        // Os valores vão como parâmetros, separados do comando: isso impede SQL Injection.
        var parametros = new DynamicParameters();
        parametros.Add("DataReserva", data.Date, DbType.Date);
        parametros.Add("HoraInicio", horaInicio, DbType.Time);
        parametros.Add("HoraFim", horaFim, DbType.Time);
        parametros.Add("ID_Usuario", usuarioId);
        parametros.Add("ID_Quadra", quadraId);
        parametros.Add("ID_Pagamento", pagamentoId);

        try
        {
            // A procedure termina com um SELECT do id do agendamento criado.
            return conexao.QuerySingle<int>(
                "CadastrarAgendamento",
                parametros,
                commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            // 50000 = erro lançado pela própria procedure com RAISERROR
            // (conflito de horário, horário inválido, vínculo inexistente).
            throw new ErroDeBancoException(ex.Message);
        }
    }
}
