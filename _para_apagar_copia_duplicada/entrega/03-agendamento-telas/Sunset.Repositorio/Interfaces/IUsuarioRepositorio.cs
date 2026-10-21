using Sunset.Modelos;

namespace Sunset.Repositorio.Interfaces;

public interface IUsuarioRepositorio
{
    Usuario? ObterPorId(int id);
    Usuario? ObterPorEmail(string email);
    bool EmailEmUso(string email);
    bool CpfEmUso(string cpf);
    void Adicionar(Usuario usuario);
}
