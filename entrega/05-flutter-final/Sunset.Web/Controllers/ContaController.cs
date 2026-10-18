using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunset.Modelos;
using Sunset.Modelos.Dtos;
using Sunset.Negocio;

namespace Sunset.Web.Controllers;

// Cadastro, login e logout. Nenhuma regra aqui: quem decide é o UsuarioNegocio.
public class ContaController : Controller
{
    private readonly UsuarioNegocio _negocio;

    public ContaController(UsuarioNegocio negocio)
    {
        _negocio = negocio;
    }

    // GET /Conta/Login
    public IActionResult Login(string? returnUrl) =>
        View(new LoginDto { }, returnUrl);

    // POST /Conta/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto modelo, string? returnUrl)
    {
        if (!ModelState.IsValid) return View(modelo, returnUrl);

        try
        {
            var usuario = _negocio.Login(modelo.Email, modelo.Senha);
            await HttpContext.EntrarNoSistema(usuario);
            return Redirecionar(returnUrl);
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo, returnUrl);
        }
    }

    // GET /Conta/Cadastro
    public IActionResult Cadastro() => View(new CadastroDto());

    // POST /Conta/Cadastro
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro(CadastroDto modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            var usuario = _negocio.Cadastrar(modelo.Nome, modelo.Sobrenome, modelo.Email, modelo.Cpf, modelo.Senha);
            await HttpContext.EntrarNoSistema(usuario);

            // Quem marcou "quero ser gestor" vai direto cadastrar a quadra.
            if (modelo.QuerSerGestor)
                return RedirectToAction("Criar", "Quadras");

            return RedirectToAction("Index", "Home");
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    // POST /Conta/Sair
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AcessoNegado() => View();

    // ---------- apoio ----------

    // Só volta para endereços do próprio site (evita redirecionar para outro site).
    private IActionResult Redirecionar(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    private ViewResult View(LoginDto modelo, string? returnUrl)
    {
        ViewData["returnUrl"] = returnUrl;
        return base.View(modelo);
    }
}
