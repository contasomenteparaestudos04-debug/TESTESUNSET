using Sunset.Modelos;
using Sunset.Modelos.Dtos;
using Sunset.Repositorio;
using Sunset.Repositorio.Interfaces;

namespace Sunset.Negocio;

public class AgendamentoNegocio
{
    // Funcionamento das quadras: horários de 1 hora, das 08h às 22h.
    public const int HoraAbertura = 8;
    public const int HoraFechamento = 22;
    private const int DiasMaximosAFrente = 60;

    private readonly IAgendamentoRepositorio _repositorio;
    private readonly IPagamentoRepositorio _pagamentos;
    private readonly IQuadraRepositorio _quadras;
    private readonly IUsuarioRepositorio _usuarios;

    public AgendamentoNegocio(IAgendamentoRepositorio repositorio, IPagamentoRepositorio pagamentos,
                              IQuadraRepositorio quadras, IUsuarioRepositorio usuarios)
    {
        _repositorio = repositorio;
        _pagamentos = pagamentos;
        _quadras = quadras;
        _usuarios = usuarios;
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

    // Fluxo do cliente: escolhe quadra, dia e horário. O sistema cria um pagamento
    // pendente (valor de 1 hora) e chama a procedure. Devolve o id do agendamento.
    public int Reservar(int usuarioId, int quadraId, DateTime data, TimeSpan horaInicio)
    {
        var quadra = _quadras.ObterPorId(quadraId)
            ?? throw new RegraDeNegocioException("Quadra não encontrada.");

        data = data.Date;
        if (horaInicio.Minutes != 0 || horaInicio.Hours < HoraAbertura || horaInicio.Hours >= HoraFechamento)
            throw new RegraDeNegocioException($"Os horários vão das {HoraAbertura:00}:00 às {HoraFechamento:00}:00, de hora em hora.");

        if (data + horaInicio <= DateTime.Now)
            throw new RegraDeNegocioException("Esse horário já passou. Escolha um horário futuro.");

        if (data > DateTime.Today.AddDays(DiasMaximosAFrente))
            throw new RegraDeNegocioException($"Só é possível agendar com até {DiasMaximosAFrente} dias de antecedência.");

        var pagamento = new Pagamento
        {
            Valor = quadra.PrecoHora,
            DataPagamento = DateTime.Now,
            MetodoPagamento = "PIX",
            StatusPagamento = StatusPagamento.Pendente,
            QuadraId = quadraId,
            UsuarioId = usuarioId
        };
        _pagamentos.Adicionar(pagamento);

        try
        {
            return Agendar(data, horaInicio, horaInicio.Add(TimeSpan.FromHours(1)),
                           usuarioId, quadraId, pagamento.Id);
        }
        catch (RegraDeNegocioException)
        {
            // Não conseguiu agendar (ex.: horário ocupado): apaga o pagamento criado à toa.
            _pagamentos.Remover(pagamento);
            throw;
        }
    }

    // Cancela um agendamento. Quem pode: o cliente que reservou, o gestor da quadra e o Admin.
    public void Cancelar(int agendamentoId, int usuarioId)
    {
        var agendamento = _repositorio.ObterPorId(agendamentoId)
            ?? throw new RegraDeNegocioException("Agendamento não encontrado.");

        var usuario = _usuarios.ObterPorId(usuarioId)
            ?? throw new RegraDeNegocioException("Usuário não encontrado.");

        var podeCancelar = agendamento.UsuarioId == usuarioId
                        || agendamento.Quadra?.GestorId == usuarioId
                        || usuario.Permissao == Perfis.Admin;
        if (!podeCancelar)
            throw new RegraDeNegocioException("Você não tem permissão para cancelar este agendamento.");

        if (agendamento.StatusReserva == StatusReserva.Cancelado)
            throw new RegraDeNegocioException("Este agendamento já está cancelado.");

        if (agendamento.StatusReserva == StatusReserva.Finalizado
            || agendamento.DataReserva.Date + agendamento.HoraInicio <= DateTime.Now)
            throw new RegraDeNegocioException("Não dá para cancelar um agendamento que já começou ou terminou.");

        agendamento.StatusReserva = StatusReserva.Cancelado;
        _repositorio.Atualizar(agendamento);

        // Pagamento ainda pendente também é cancelado. (Devolução de pagamento aprovado
        // não faz parte do PIM: fica como pendência.)
        var pagamento = agendamento.Pagamento;
        if (pagamento != null && pagamento.StatusPagamento == StatusPagamento.Pendente)
        {
            pagamento.StatusPagamento = StatusPagamento.Cancelado;
            _pagamentos.Atualizar(pagamento);
        }
    }

    // ---------- consultas para as telas ----------

    public List<AgendamentoDto> MeusAgendamentos(int usuarioId) =>
        _repositorio.ListarPorUsuario(usuarioId).Select(AgendamentoDto.De).ToList();

    // Os 14 horários do dia, marcando os ocupados.
    public List<HorarioDto> HorariosDoDia(int quadraId, DateTime dia)
    {
        var ocupados = _repositorio.ListarDaQuadraNoDia(quadraId, dia);
        var lista = new List<HorarioDto>();

        for (int hora = HoraAbertura; hora < HoraFechamento; hora++)
        {
            var inicio = TimeSpan.FromHours(hora);
            var fim = TimeSpan.FromHours(hora + 1);
            // Mesma regra de cruzamento da procedure.
            var agendamento = ocupados.FirstOrDefault(a => inicio < a.HoraFim && fim > a.HoraInicio);

            lista.Add(new HorarioDto
            {
                Inicio = inicio.ToString(@"hh\:mm"),
                Fim = fim.ToString(@"hh\:mm"),
                Ocupado = agendamento != null,
                Passado = dia.Date + inicio <= DateTime.Now,
                AgendamentoId = agendamento?.Id,
                Cliente = agendamento?.Usuario?.NomeSobrenome
            });
        }
        return lista;
    }

    // Lista as quadras de um esporte dizendo se estão livres neste momento.
    public List<QuadraStatusDto> QuadrasPorEsporte(string? esporte)
    {
        var quadras = _quadras.Listar();
        if (!string.IsNullOrEmpty(esporte))
            quadras = quadras.Where(q => q.Categoria == esporte).ToList();

        var agora = DateTime.Now;
        return quadras.Select(q => new QuadraStatusDto
        {
            Quadra = QuadraDto.De(q),
            LivreAgora = !_repositorio.ListarDaQuadraNoDia(q.Id, agora)
                .Any(a => agora.TimeOfDay >= a.HoraInicio && agora.TimeOfDay < a.HoraFim)
        }).ToList();
    }

    public QuadraHorariosDto? HorariosDaQuadra(int quadraId, DateTime dia)
    {
        var quadra = _quadras.ObterPorId(quadraId);
        if (quadra is null) return null;

        return new QuadraHorariosDto
        {
            Quadra = QuadraDto.De(quadra),
            Data = dia.Date,
            Horarios = HorariosDoDia(quadraId, dia)
        };
    }

    // Agenda dos próximos dias das quadras do gestor (o Admin vê todas).
    public List<AgendaQuadraDto> AgendaDoGestor(int usuarioId, string perfil, int dias = 5)
    {
        var quadras = perfil == Perfis.Admin ? _quadras.Listar() : _quadras.ListarPorGestor(usuarioId);

        return quadras.Select(q => new AgendaQuadraDto
        {
            Quadra = QuadraDto.De(q),
            Dias = Enumerable.Range(0, dias).Select(i => new AgendaDiaDto
            {
                Data = DateTime.Today.AddDays(i),
                Horarios = HorariosDoDia(q.Id, DateTime.Today.AddDays(i))
            }).ToList()
        }).ToList();
    }
}
