using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunset.Negocio;

namespace Sunset.Web.Controllers;

// Painel do administrador: usuários, agendamentos e relatório.
// [Authorize(Roles = "Admin")] já barra qualquer outro perfil.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UsuarioNegocio _usuarios;
    private readonly AgendamentoNegocio _agendamentos;
    private readonly RelatorioNegocio _relatorio;

    public AdminController(UsuarioNegocio usuarios, AgendamentoNegocio agendamentos, RelatorioNegocio relatorio)
    {
        _usuarios = usuarios;
        _agendamentos = agendamentos;
        _relatorio = relatorio;
    }

    public IActionResult Index() => View();

    // GET /Admin/Usuarios?busca=ana
    public IActionResult Usuarios(string? busca)
    {
        ViewData["Busca"] = busca;
        return View(_usuarios.ListarParaAdmin(busca));
    }

    // POST /Admin/ExcluirUsuario
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirUsuario(int id)
    {
        try
        {
            _usuarios.Excluir(User.Id(), id);
            TempData["Mensagem"] = "Usuário excluído.";
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Usuarios));
    }

    // GET /Admin/Agendamentos?busca=arena&de=2026-12-01&ate=2026-12-31
    public IActionResult Agendamentos(string? busca, DateTime? de, DateTime? ate)
    {
        ViewData["Busca"] = busca;
        ViewData["De"] = de?.ToString("yyyy-MM-dd");
        ViewData["Ate"] = ate?.ToString("yyyy-MM-dd");
        return View(_agendamentos.ListarParaAdmin(busca, de, ate));
    }

    // POST /Admin/ExcluirAgendamento
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirAgendamento(int id)
    {
        try
        {
            _agendamentos.ExcluirComoAdmin(id, User.Id());
            TempData["Mensagem"] = "Agendamento excluído.";
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Agendamentos));
    }

    // GET /Admin/Relatorio   -> baixa o relatorio-sunset.txt
    public IActionResult Relatorio()
    {
        var bytes = Encoding.UTF8.GetBytes(_relatorio.Gerar());
        return File(bytes, "text/plain; charset=utf-8", "relatorio-sunset.txt");
    }
}
