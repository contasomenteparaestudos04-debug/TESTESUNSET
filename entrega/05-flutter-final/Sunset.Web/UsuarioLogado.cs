using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sunset.Modelos;

namespace Sunset.Web;

// Atalhos para o usuário logado (dados guardados no cookie de login).
public static class UsuarioLogado
{
    public static int Id(this ClaimsPrincipal usuario) =>
        int.Parse(usuario.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static string Perfil(this ClaimsPrincipal usuario) =>
        usuario.FindFirstValue(ClaimTypes.Role) ?? Perfis.Cliente;

    // Grava (ou troca) o cookie de login com id, nome, e-mail e perfil.
    public static async Task EntrarNoSistema(this HttpContext http, Usuario usuario)
    {
        var dados = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NomeSobrenome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Permissao)
        };
        var identidade = new ClaimsIdentity(dados, CookieAuthenticationDefaults.AuthenticationScheme);
        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidade));
    }
}
