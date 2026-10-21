using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunset.Modelos;
using Sunset.Negocio;

namespace Sunset.Web.Controllers;

// Telas de agendamento: escolher quadra e horário, ver e cancelar meus agendamentos
// e a agenda do gestor. As regras ficam no AgendamentoNegocio.
[Authorize]
public class ReservasController : Controller
{
    private readonly AgendamentoNegocio _negocio;
    private readonly RelatorioNegocio _relatorio;

    public ReservasController(AgendamentoNegocio negocio, RelatorioNegocio relatorio)
    {
        _negocio = negocio;
        _relatorio = relatorio;
    }

    // GET /Reservas?esporte=Volei   -> quadras do esporte, livres ou ocupadas agora
    public IActionResult Index(string? esporte)
    {
        if (string.IsNullOrEmpty(esporte)) esporte = Categorias.Volei;
        ViewData["Esporte"] = esporte;
        return View(_negocio.QuadrasPorEsporte(esporte));
    }

    // GET /Reservas/Horarios/3?data=2026-10-20
    public IActionResult Horarios(int id, DateTime? data)
    {
        var dia = (data ?? DateTime.Today).Date;
        if (dia < DateTime.Today) dia = DateTime.Today;

        var modelo = _negocio.HorariosDaQuadra(id, dia);
        if (modelo is null) return NotFound();
        return View(modelo);
    }

    // POST /Reservas/Reservar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reservar(int quadraId, DateTime data, TimeSpan hora)
    {
        try
        {
            var reserva = _negocio.Reservar(User.Id(), quadraId, data, hora);
            // Reservou: agora falta pagar o Pix (prazo de 5 minutos).
            return RedirectToAction("Pix", "Pagamentos", new { id = reserva.PagamentoId });
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(Horarios), new { id = quadraId, data = data.ToString("yyyy-MM-dd") });
        }
    }

    // GET /Reservas/Minhas
    public IActionResult Minhas() => View(_negocio.MeusAgendamentos(User.Id()));

    // POST /Reservas/Cancelar   (o cliente cancela o dele; o gestor cancela os da quadra dele)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(int id, string? voltar)
    {
        try
        {
            _negocio.Cancelar(id, User.Id());
            TempData["Mensagem"] = "Agendamento cancelado.";
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return voltar == "agenda" ? RedirectToAction(nameof(Agenda)) : RedirectToAction(nameof(Minhas));
    }

    // GET /Reservas/Agenda   -> próximos 5 dias das quadras do gestor
    [Authorize(Roles = "Gestor,Admin")]
    public IActionResult Agenda() => View(_negocio.AgendaDoGestor(User.Id(), User.Perfil()));

    // GET /Reservas/Relatorio?de=2026-10-01&ate=2026-10-31   -> baixa o relatorio-quadras.txt
    // O relatório usa o id do usuário logado, então o gestor só recebe as quadras dele.
    [Authorize(Roles = "Gestor,Admin")]
    public IActionResult Relatorio(DateTime? de, DateTime? ate)
    {
        if (de.HasValue && ate.HasValue && ate.Value < de.Value)
        {
            TempData["Erro"] = "A data final não pode ser antes da data inicial.";
            return RedirectToAction(nameof(Agenda));
        }

        var texto = _relatorio.GerarDoGestor(User.Id(), User.Perfil(), de, ate);
        return File(System.Text.Encoding.UTF8.GetBytes(texto), "text/plain; charset=utf-8", "relatorio-quadras.txt");
    }
}
