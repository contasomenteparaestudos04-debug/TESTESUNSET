namespace Sunset.Modelos.Dtos;

// DTO de SAÍDA: um agendamento como aparece nas telas e no JSON da Api.
public class AgendamentoDto
{
    public int Id { get; set; }
    public int QuadraId { get; set; }
    public string Quadra { get; set; } = string.Empty;
    public string Esporte { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFim { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool PodeCancelar { get; set; }

    public static AgendamentoDto De(Agendamento a) => new()
    {
        Id = a.Id,
        QuadraId = a.QuadraId,
        Quadra = a.Quadra?.NomeQuadra ?? "",
        Esporte = a.Quadra?.Categoria ?? "",
        Cliente = a.Usuario?.NomeSobrenome ?? "",
        Data = a.DataReserva,
        HoraInicio = a.HoraInicio.ToString(@"hh\:mm"),
        HoraFim = a.HoraFim.ToString(@"hh\:mm"),
        Status = a.StatusReserva,
        // Só dá para cancelar o que ainda não terminou nem foi cancelado e ainda não começou.
        PodeCancelar = a.StatusReserva != StatusReserva.Cancelado
                    && a.StatusReserva != StatusReserva.Finalizado
                    && a.DataReserva.Date + a.HoraInicio > DateTime.Now
    };
}

// Um horário de 1 hora de uma quadra, livre ou ocupado.
public class HorarioDto
{
    public string Inicio { get; set; } = string.Empty;
    public string Fim { get; set; } = string.Empty;
    public bool Ocupado { get; set; }
    // Horário que já passou hoje: não dá mais para reservar.
    public bool Passado { get; set; }
    public int? AgendamentoId { get; set; }
    public string? Cliente { get; set; }
}

// Quadra da lista do cliente com a situação neste momento.
public class QuadraStatusDto
{
    public QuadraDto Quadra { get; set; } = new();
    public bool LivreAgora { get; set; }
}

// Dados da tela "escolher horário".
public class QuadraHorariosDto
{
    public QuadraDto Quadra { get; set; } = new();
    public DateTime Data { get; set; }
    public List<HorarioDto> Horarios { get; set; } = new();
}

// Um dia da agenda do gestor.
public class AgendaDiaDto
{
    public DateTime Data { get; set; }
    public List<HorarioDto> Horarios { get; set; } = new();
}

public class AgendaQuadraDto
{
    public QuadraDto Quadra { get; set; } = new();
    public List<AgendaDiaDto> Dias { get; set; } = new();
}

// DTO de ENTRADA da Api: reservar um horário (a Api cria o pagamento pendente sozinha).
public class ReservaDto
{
    public int UsuarioId { get; set; }
    public int QuadraId { get; set; }
    public DateTime Data { get; set; }
    // Formato "18:00:00"
    public TimeSpan HoraInicio { get; set; }
}

// DTO de ENTRADA da Api: quem está cancelando.
public class CancelarDto
{
    public int UsuarioId { get; set; }
}
