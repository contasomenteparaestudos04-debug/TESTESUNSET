using System.Text.RegularExpressions;
using Sunset.Modelos;
using Sunset.Repositorio.Interfaces;

namespace Sunset.Negocio;

// Regras de cadastro e login. Web e Api chamam esta classe.
public class UsuarioNegocio
{
    private readonly IUsuarioRepositorio _repositorio;

    public UsuarioNegocio(IUsuarioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public Usuario? ObterPorId(int id) => _repositorio.ObterPorId(id);

    // Todo cadastro novo entra como Cliente. Quem cadastra uma quadra vira Gestor
    // (a trigger trg_VirarGestor faz isso no banco). Admin só existe pela migration.
    public Usuario Cadastrar(string nome, string sobrenome, string email, string cpf, string senha)
    {
        nome = Texto.RemoverAcentos(nome.Trim());
        sobrenome = Texto.RemoverAcentos(sobrenome.Trim());
        email = email.Trim().ToLowerInvariant();
        cpf = Texto.SoDigitos(cpf);

        var nomeCompleto = $"{nome} {sobrenome}";
        if (!Texto.SoLetrasNumerosEspacos(nomeCompleto))
            throw new RegraDeNegocioException("Nome e sobrenome aceitam só letras, números e espaços.");
        if (nomeCompleto.Length > 100)
            throw new RegraDeNegocioException("Nome e sobrenome juntos podem ter no máximo 100 caracteres.");

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]{2,}\.[^@\s.]{2,}$"))
            throw new RegraDeNegocioException("E-mail inválido.");

        if (!Validacao.CpfValido(cpf))
            throw new RegraDeNegocioException("CPF inválido.");

        // CA1 da documentação: senha com no mínimo 8 caracteres.
        if (senha.Length < 8)
            throw new RegraDeNegocioException("A senha precisa ter no mínimo 8 caracteres.");

        if (_repositorio.EmailEmUso(email))
            throw new RegraDeNegocioException("Já existe um usuário com este e-mail.");

        if (_repositorio.CpfEmUso(cpf))
            throw new RegraDeNegocioException("Já existe um usuário com este CPF.");

        var usuario = new Usuario
        {
            NomeSobrenome = nomeCompleto,
            Email = email,
            Cpf = cpf,
            Senha = Seguranca.GerarHash(senha),
            Permissao = Perfis.Cliente,
            StatusUsuario = "Ativo"
        };

        _repositorio.Adicionar(usuario);
        return usuario;
    }

    // Confere e-mail e senha. A mensagem é a mesma nos dois erros, como na documentação,
    // para não revelar se o e-mail existe.
    public Usuario Login(string email, string senha)
    {
        var usuario = _repositorio.ObterPorEmail(email.Trim().ToLowerInvariant());

        if (usuario is null || usuario.Senha != Seguranca.GerarHash(senha))
            throw new RegraDeNegocioException("Email ou senha incorretos");

        if (usuario.StatusUsuario != "Ativo")
            throw new RegraDeNegocioException("Esta conta está " + usuario.StatusUsuario.ToLower() + ".");

        return usuario;
    }
}
