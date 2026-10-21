using Sunset.Modelos;
using Sunset.Repositorio.Contexto;
using Sunset.Repositorio.Interfaces;

namespace Sunset.Repositorio;

// Única classe que conversa com a tabela Usuario.
public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly AppDbContext _contexto;

    public UsuarioRepositorio(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Usuario? ObterPorId(int id) =>
        _contexto.Usuarios.FirstOrDefault(u => u.Id == id);

    public Usuario? ObterPorEmail(string email) =>
        _contexto.Usuarios.FirstOrDefault(u => u.Email == email);

    public bool EmailEmUso(string email) =>
        _contexto.Usuarios.Any(u => u.Email == email);

    public bool CpfEmUso(string cpf) =>
        _contexto.Usuarios.Any(u => u.Cpf == cpf);

    public void Adicionar(Usuario usuario)
    {
        _contexto.Usuarios.Add(usuario);
        _contexto.SaveChanges();
    }
}
