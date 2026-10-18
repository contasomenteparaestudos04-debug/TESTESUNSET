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

    public ReservasController(AgendamentoNegocio negocio)
    {
        _negocio = negocio;
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
            _negocio.Reservar(User.Id(), quadraId, data, hora);
            TempData["Mensagem"] = "Horário reservado! Veja em Meus agendamentos.";
            return RedirectToAction(nameof(Minhas));
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
}
