using Sunset.Modelos;
using Sunset.Modelos.Dtos;
using Sunset.Negocio;
using Sunset.Repositorio;
using Sunset.Repositorio.Interfaces;

class FakeQuadras : IQuadraRepositorio {
    public List<Quadra> Itens = new();
    public List<Quadra> Listar() => Itens;
    public List<Quadra> ListarPorGestor(int g) => Itens.Where(q => q.GestorId == g).ToList();
    public Quadra? ObterPorId(int id) => Itens.FirstOrDefault(q => q.Id == id);
    public void Adicionar(Quadra q) { q.Id = Itens.Count + 1; Itens.Add(q); }
    public void Atualizar(Quadra q) { }
    public void Remover(Quadra q) { Itens.Remove(q); }
}
class FakeAg : IAgendamentoRepositorio {
    public List<Agendamento> Itens = new();
    public int CadastrarPelaProcedure(DateTime d, TimeSpan i, TimeSpan f, int u, int q, int p) {
        if (u == 999) throw new ErroDeBancoException("Erro de Vínculo: ...");
        if (Itens.Any(a => a.QuadraId == q && a.DataReserva == d && a.StatusReserva != "Cancelado" && i < a.HoraFim && f > a.HoraInicio))
            throw new ErroDeBancoException("Erro: Esta quadra já possui um agendamento para o horário selecionado.");
        var ag = new Agendamento { Id = Itens.Count + 7, DataReserva = d, HoraInicio = i, HoraFim = f, UsuarioId = u, QuadraId = q, PagamentoId = p };
        ag.Quadra = Quadras.FirstOrDefault(x => x.Id == q);
        ag.Pagamento = Pags.Itens.FirstOrDefault(x => x.Id == p);
        Itens.Add(ag);
        return ag.Id;
    }
    public static List<Quadra> Quadras = new();
    public static FakePag Pags = new();
    public Agendamento? ObterPorId(int id) => Itens.FirstOrDefault(a => a.Id == id);
    public List<Agendamento> ListarPorUsuario(int u) => Itens.Where(a => a.UsuarioId == u).ToList();
    public List<Agendamento> ListarDaQuadraNoDia(int q, DateTime d) => Itens.Where(a => a.QuadraId == q && a.DataReserva == d.Date && a.StatusReserva != "Cancelado").ToList();
    public void Atualizar(Agendamento a) { }
    public Agendamento? ObterPorPagamento(int p) => Itens.FirstOrDefault(a => a.PagamentoId == p);
    public List<Agendamento> ListarPendentesVencidos(DateTime limite) => Itens.Where(a => a.StatusReserva == "Pendente" && a.Pagamento != null && a.Pagamento.StatusPagamento == "Pendente" && a.Pagamento.DataPagamento < limite).ToList();
    public List<Agendamento> ListarTodos(string? b, DateTime? de, DateTime? ate) => Itens.Where(a => (string.IsNullOrEmpty(b) || (a.Usuario?.NomeSobrenome ?? "").Contains(b)) && (!de.HasValue || a.DataReserva >= de) && (!ate.HasValue || a.DataReserva <= ate)).ToList();
    public List<Agendamento> ListarDasQuadras(List<int> ids, DateTime? de, DateTime? ate) => Itens.Where(a => ids.Contains(a.QuadraId) && (!de.HasValue || a.DataReserva >= de.Value.Date) && (!ate.HasValue || a.DataReserva <= ate.Value.Date)).OrderBy(a => a.DataReserva).ThenBy(a => a.HoraInicio).ToList();
    public void Remover(Agendamento a) { Itens.Remove(a); }
}
class FakePag : IPagamentoRepositorio {
    public List<Pagamento> Itens = new();
    public Pagamento? ObterPorId(int id) => Itens.FirstOrDefault(p => p.Id == id);
    public void Adicionar(Pagamento p) { p.Id = Itens.Count + 1; Itens.Add(p); }
    public void Atualizar(Pagamento p) { }
    public void Remover(Pagamento p) { Itens.Remove(p); }
}
class FakeUsu : IUsuarioRepositorio {
    public List<Usuario> Itens = new();
    public Usuario? ObterPorId(int id) => Itens.FirstOrDefault(u => u.Id == id);
    public Usuario? ObterPorEmail(string e) => Itens.FirstOrDefault(u => u.Email == e);
    public bool EmailEmUso(string e) => Itens.Any(u => u.Email == e);
    public bool CpfEmUso(string c) => Itens.Any(u => u.Cpf == c);
    public void Adicionar(Usuario u) { u.Id = Itens.Count + 1; Itens.Add(u); }
    public List<Usuario> Listar(string? n) => Itens.Where(u => string.IsNullOrEmpty(n) || u.Email.Contains(n)).ToList();
    public void Excluir(int exec, int id) {
        var ex = Itens.FirstOrDefault(u => u.Id == exec);
        if (ex == null || ex.Permissao != "Admin") throw new ErroDeBancoException("Ação negada: Apenas administradores podem realizar essa função");
        if (id == 55) throw new ErroDeBancoException("Não dá para excluir: o usuário tem agendamentos, pagamentos ou quadras.");
        Itens.RemoveAll(u => u.Id == id);
    }
}
class T {
    static int falhas = 0;
    static void Ok(bool cond, string nome) { Console.WriteLine((cond ? "OK   " : "FALHA ") + nome); if (!cond) falhas++; }
    static bool Lanca(Action a, string trecho) { try { a(); return false; } catch (RegraDeNegocioException e) { return e.Message.Contains(trecho); } }
    static void Main() {
        var repo = new FakeQuadras(); var n = new QuadraNegocio(repo);
        var dto = new QuadraSalvarDto { NomeQuadra = "Arena São João 1", PrecoHora = 150, Categoria = "Volei", Cep = "14020-250", Logradouro = "Avenida João Fiúsa", Numero = "", Bairro = "Jardim Botânico", Cidade = "Ribeirão Preto", Estado = "SP" };
        var q = n.Cadastrar(dto.ParaQuadra());
        Ok(q.NomeQuadra == "Arena Sao Joao 1", "remove acentos do nome");
        Ok(q.Endereco!.Cep == "14020250", "CEP só dígitos");
        Ok(q.Endereco.Numero == "S/N", "sem número vira S/N");
        Ok(q.Endereco.Cidade == "Ribeirao Preto", "cidade sem acento");
        dto.NomeQuadra = "Quadra ###"; Ok(Lanca(() => n.Cadastrar(dto.ParaQuadra()), "letras, números"), "nome com caracteres especiais recusado");
        dto.NomeQuadra = "Q1"; dto.PrecoHora = 0; Ok(Lanca(() => n.Cadastrar(dto.ParaQuadra()), "maior que zero"), "preço zero recusado");
        dto.PrecoHora = 10; dto.Categoria = "Futebol"; Ok(Lanca(() => n.Cadastrar(dto.ParaQuadra()), "Esporte"), "esporte inválido recusado");
        dto.Categoria = "Volei"; dto.Cep = "123"; Ok(Lanca(() => n.Cadastrar(dto.ParaQuadra()), "8 dígitos"), "CEP curto recusado");
        Ok(n.Remover(1) && !n.Remover(99), "remover existente/inexistente");
        var fagFixo = new FakeAg();
        var a = new AgendamentoNegocio(fagFixo, new FakePag(), repo, new FakeUsu());
        Ok(a.Agendar(DateTime.Today, TimeSpan.FromHours(18), TimeSpan.FromHours(19), 1, 1, 1) == 7, "agendar devolve id da procedure");
        Ok(Lanca(() => a.Agendar(DateTime.Today, TimeSpan.FromHours(20), TimeSpan.FromHours(19), 1, 1, 1), "término"), "hora fim <= início recusada");
        Ok(Lanca(() => a.Agendar(DateTime.Today, TimeSpan.FromHours(18), TimeSpan.FromHours(19), 999, 1, 1), "Vínculo"), "erro da procedure vira regra de negócio");
        Console.WriteLine(falhas == 0 ? "TUDO OK" : $"{falhas} FALHA(S)");
        var un = new UsuarioNegocio(new FakeUsu());
        var u = un.Cadastrar("João", "Silva", " Joao@Teste.com ", "123.456.789-09", "Senha1234");
        Ok(u.NomeSobrenome == "Joao Silva" && u.Email == "joao@teste.com" && u.Permissao == "Cliente", "cadastro normaliza e vira Cliente");
        Ok(u.Senha == "5b5ecb4a0f0ab39b2cb4ae7f5a1b3d6c" || u.Senha.Length == 64, "senha guardada como hash SHA256");
        Ok(Lanca(() => un.Cadastrar("A","B","joao@teste.com","529.982.247-25","Senha1234"), "e-mail"), "e-mail repetido recusado");
        Ok(Lanca(() => un.Cadastrar("A","B","ana@bb.com","111.111.111-11","Senha1234"), "CPF inválido"), "CPF inválido recusado");
        Ok(Lanca(() => un.Cadastrar("A","B","ana@bb.com","529.982.247-25","curta"), "8 caracteres"), "senha curta recusada");
        Ok(Lanca(() => un.Cadastrar("A","B","semarroba","529.982.247-25","Senha1234"), "E-mail inválido"), "e-mail inválido recusado");
        Ok(un.Login("JOAO@teste.com", "Senha1234").Id == u.Id, "login correto");
        Ok(Lanca(() => un.Login("joao@teste.com", "errada123"), "incorretos"), "senha errada recusada");
        Ok(Lanca(() => un.Login("nao@existe.com", "Senha1234"), "incorretos"), "e-mail inexistente recusado (mesma msg)");
        Console.WriteLine(falhas == 0 ? "TUDO OK" : $"{falhas} FALHAS");
        // ---- Pasta 3: reservar e cancelar ----
        var qrepo = new FakeQuadras(); var upr = new FakeUsu(); var pr = new FakePag(); FakeAg.Pags = pr;
        var cli = new Usuario { Id = 1, Permissao = "Cliente", Email = "c@x.com" };
        var outro = new Usuario { Id = 2, Permissao = "Cliente", Email = "o@x.com" };
        var gest = new Usuario { Id = 3, Permissao = "Gestor", Email = "g@x.com" };
        var adm = new Usuario { Id = 4, Permissao = "Admin", Email = "a@x.com" };
        upr.Itens.AddRange(new[] { cli, outro, gest, adm });
        var quad = new Quadra { Id = 1, NomeQuadra = "Q", PrecoHora = 80, Categoria = "Volei", GestorId = 3 };
        qrepo.Itens.Add(quad); FakeAg.Quadras = qrepo.Itens;
        var fag = new FakeAg(); var an = new AgendamentoNegocio(fag, pr, qrepo, upr);
        var amanha = DateTime.Today.AddDays(1);
        var res1 = an.Reservar(1, 1, amanha, TimeSpan.FromHours(18)); var id1 = res1.AgendamentoId;
        Ok(pr.Itens.Count == 1 && pr.Itens[0].Valor == 80 && pr.Itens[0].StatusPagamento == "Pendente", "reservar cria pagamento pendente com o preço da hora");
        Ok(fag.Itens.Count == 1 && fag.Itens[0].HoraFim == TimeSpan.FromHours(19), "reserva dura 1 hora");
        Ok(Lanca(() => an.Reservar(2, 1, amanha, TimeSpan.FromHours(18)), "já possui"), "mesmo horário recusado");
        Ok(pr.Itens.Count == 1, "pagamento do pedido recusado foi apagado");
        Ok(Lanca(() => an.Reservar(1, 1, DateTime.Today.AddDays(-1), TimeSpan.FromHours(18)), "já passou"), "data passada recusada");
        Ok(Lanca(() => an.Reservar(1, 1, amanha, TimeSpan.FromHours(23)), "08:00"), "fora do funcionamento recusado");
        Ok(Lanca(() => an.Reservar(1, 1, amanha, new TimeSpan(18, 30, 0)), "08:00"), "horário quebrado recusado");
        Ok(Lanca(() => an.Reservar(1, 1, DateTime.Today.AddDays(90), TimeSpan.FromHours(10)), "antecedência"), "mais de 60 dias recusado");
        var h = an.HorariosDoDia(1, amanha);
        Ok(h.Count == 14 && h.Single(x => x.Inicio == "18:00").Ocupado && !h.Single(x => x.Inicio == "17:00").Ocupado, "grade de 14 horários marca o ocupado");
        Ok(Lanca(() => an.Cancelar(id1, 2), "permissão"), "outro cliente não cancela");
        an.Cancelar(id1, 1);
        Ok(fag.Itens[0].StatusReserva == "Cancelado" && pr.Itens[0].StatusPagamento == "Cancelado", "cliente cancela e pagamento pendente é cancelado");
        Ok(Lanca(() => an.Cancelar(id1, 1), "já está cancelado"), "cancelar duas vezes recusado");
        Ok(!an.HorariosDoDia(1, amanha).Single(x => x.Inicio == "18:00").Ocupado, "horário cancelado volta a ficar livre");
        var id2 = an.Reservar(1, 1, amanha, TimeSpan.FromHours(18)).AgendamentoId;
        an.Cancelar(id2, 3);
        Ok(fag.Itens.Last().StatusReserva == "Cancelado", "gestor da quadra cancela");
        var id3 = an.Reservar(1, 1, amanha, TimeSpan.FromHours(18)).AgendamentoId;
        an.Cancelar(id3, 4);
        Ok(fag.Itens.Last().StatusReserva == "Cancelado", "admin cancela");
        var qn = new QuadraNegocio(qrepo);
        Ok(qn.PodeGerenciar(quad, 3, "Gestor") && !qn.PodeGerenciar(quad, 2, "Gestor") && qn.PodeGerenciar(quad, 9, "Admin"), "só o dono ou Admin gerencia a quadra");
        var ag = an.AgendaDoGestor(3, "Gestor");
        Ok(ag.Count == 1 && ag[0].Dias.Count == 5, "agenda do gestor tem 5 dias");
        Ok(an.QuadrasPorEsporte("Volei").Count == 1 && an.QuadrasPorEsporte("Beach Tennis").Count == 0, "lista por esporte");
        Console.WriteLine(falhas == 0 ? "TUDO OK (2)" : $"{falhas} FALHAS");
        // ---- Pasta 4: Pix, expiração, admin, relatório ----
        var fag4 = new FakeAg(); var pr4 = new FakePag(); FakeAg.Pags = pr4;
        var qr4 = new FakeQuadras(); qr4.Itens.Add(new Quadra { Id = 1, NomeQuadra = "Q", PrecoHora = 80, Categoria = "Volei", GestorId = 3 }); FakeAg.Quadras = qr4.Itens;
        var up4 = new FakeUsu(); up4.Itens.AddRange(new[] { new Usuario { Id = 1, NomeSobrenome = "Ana", Permissao = "Cliente", Email = "ana@x.com" }, new Usuario { Id = 2, NomeSobrenome = "Bia", Permissao = "Cliente", Email = "bia@x.com" }, new Usuario { Id = 4, NomeSobrenome = "Adm", Permissao = "Admin", Email = "adm@x.com" } });
        var n4 = new AgendamentoNegocio(fag4, pr4, qr4, up4);
        var dia = DateTime.Today.AddDays(2);
        var r = n4.Reservar(1, 1, dia, TimeSpan.FromHours(10));
        var pix = n4.ObterPix(r.PagamentoId, 1);
        Ok(pix.Valor == 80 && pix.SegundosRestantes > 290 && pix.SegundosRestantes <= 300, "Pix mostra valor e ~5 minutos");
        Ok(pix.Codigo.Contains("SIMULADO"), "código Pix é de exemplo");
        Ok(Lanca(() => n4.ObterPix(r.PagamentoId, 2), "não encontrado"), "outro usuário não vê o Pix");
        n4.SimularPagamento(r.PagamentoId, 1);
        Ok(pr4.Itens[0].StatusPagamento == "Aprovado" && fag4.Itens[0].StatusReserva == "Em andamento", "simular aprova pagamento e confirma reserva");
        Ok(Lanca(() => n4.SimularPagamento(r.PagamentoId, 1), "já foi aprovado"), "pagar duas vezes recusado");
        // expiração: reserva com pagamento criado há 6 minutos
        var r2 = n4.Reservar(2, 1, dia, TimeSpan.FromHours(12));
        pr4.Itens.Single(p => p.Id == r2.PagamentoId).DataPagamento = DateTime.Now.AddMinutes(-6);
        Ok(Lanca(() => n4.SimularPagamento(r2.PagamentoId, 2), "tempo do Pix acabou"), "pagar depois de 5 minutos é recusado no servidor");
        Ok(fag4.Itens.Single(a => a.Id == r2.AgendamentoId).StatusReserva == "Cancelado", "reserva vencida é cancelada");
        Ok(!n4.HorariosDoDia(1, dia).Single(x => x.Inicio == "12:00").Ocupado, "horário vencido volta a ficar livre");
        var r3 = n4.Reservar(2, 1, dia, TimeSpan.FromHours(12));
        Ok(r3.AgendamentoId != r2.AgendamentoId, "outra pessoa consegue reservar o horário liberado");
        pr4.Itens.Single(p => p.Id == r3.PagamentoId).DataPagamento = DateTime.Now.AddMinutes(-6);
        var minhas = n4.MeusAgendamentos(2);
        Ok(minhas.All(m => !m.PodePagar), "PodePagar falso depois do prazo");
        // admin
        Ok(Lanca(() => n4.ExcluirComoAdmin(r.AgendamentoId, 1), "apenas administradores"), "cliente não exclui agendamento");
        n4.ExcluirComoAdmin(r.AgendamentoId, 4);
        Ok(!fag4.Itens.Any(a => a.Id == r.AgendamentoId) && !pr4.Itens.Any(p => p.Id == r.PagamentoId), "admin exclui agendamento e pagamento");
        var un4 = new UsuarioNegocio(up4);
        Ok(Lanca(() => un4.Excluir(4, 4), "própria conta"), "admin não exclui a si mesmo");
        Ok(Lanca(() => un4.Excluir(1, 2), "negada"), "cliente não exclui usuário (trigger)");
        un4.Excluir(4, 2);
        Ok(up4.Itens.All(u => u.Id != 2), "admin exclui usuário");
        up4.Itens.Add(new Usuario { Id = 55, Permissao = "Cliente", Email = "z@x.com" });
        Ok(Lanca(() => un4.Excluir(4, 55), "tem agendamentos"), "usuário com vínculos não é excluído");
        Ok(Lanca(() => un4.Excluir(4, 99), "não encontrado"), "usuário inexistente");
        var rel = new RelatorioNegocio(up4, qr4, fag4).Gerar();
        Ok(rel.Contains("RELATORIO GERAL") && rel.Contains("Faturamento"), "relatório TXT gerado");
        // relatório do gestor: cada gestor só recebe as quadras dele
        qr4.Itens.Add(new Quadra { Id = 2, NomeQuadra = "OutraQuadra", PrecoHora = 90, Categoria = "Futsal", GestorId = 6 });
        fag4.Itens.Add(new Agendamento { Id = 500, DataReserva = dia, HoraInicio = TimeSpan.FromHours(9), HoraFim = TimeSpan.FromHours(10), QuadraId = 2, UsuarioId = 1, Quadra = qr4.Itens[1], Usuario = up4.Itens.First(u => u.Id == 1), StatusReserva = "Finalizado" });
        var rn = new RelatorioNegocio(up4, qr4, fag4);
        var relA = rn.GerarDoGestor(3, "Gestor", null, null);
        Ok(relA.Contains("RELATORIO DAS QUADRAS") && relA.Contains("Q (Volei)"), "relatório do gestor traz a quadra dele");
        Ok(!relA.Contains("OutraQuadra") && !relA.Contains("#500"), "relatório do gestor não traz quadra nem agendamento de outro gestor");
        var relB = rn.GerarDoGestor(6, "Gestor", null, null);
        Ok(relB.Contains("OutraQuadra") && relB.Contains("#500") && !relB.Contains("Q (Volei)"), "o outro gestor só vê o que é dele");
        var relAdm = rn.GerarDoGestor(4, "Admin", null, null);
        Ok(relAdm.Contains("Q (Volei)") && relAdm.Contains("OutraQuadra"), "admin que abre o relatório das quadras vê todas");
        var relPeriodo = rn.GerarDoGestor(6, "Gestor", dia.AddDays(10), null);
        Ok(relPeriodo.Contains("Nenhum agendamento no periodo"), "filtro de período exclui agendamentos fora dele");
        var relDentro = rn.GerarDoGestor(6, "Gestor", dia, dia);
        Ok(relDentro.Contains("#500") && relDentro.Contains("Finalizado: 1"), "filtro de período inclui o dia pedido");
        Ok(rn.GerarDoGestor(77, "Gestor", null, null).Contains("Nenhuma quadra cadastrada"), "gestor sem quadras recebe relatório vazio");
        Console.WriteLine(falhas == 0 ? "TUDO OK (3)" : $"{falhas} FALHAS");
    }
}
