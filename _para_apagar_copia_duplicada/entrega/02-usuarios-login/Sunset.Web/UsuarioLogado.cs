using System.Security.Claims;

namespace Sunset.Web;

// Atalho para pegar o id do usuário logado a partir do cookie de login.
public static class UsuarioLogado
{
    public static int Id(this ClaimsPrincipal usuario) =>
        int.Parse(usuario.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
