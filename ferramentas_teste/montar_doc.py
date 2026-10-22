import os, sys, copy
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from montar_doc_parte1 import *

BASE = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..'))
doc = abrir(os.path.join(BASE, 'Documentacao_PIM.docx'))
ps = list(doc.paragraphs)
tabs = list(doc.tables)

def trocar(par, novo):
    """Troca o texto do parágrafo mantendo a formatação do primeiro trecho."""
    runs = par.runs
    runs[0].text = novo
    for r in runs[1:]:
        r._r.getparent().remove(r._r)

def celula(tabela, lin, col, novo):
    c = tabela.rows[lin].cells[col]
    trocar(c.paragraphs[0], novo)

p_conclusao = ps[442]
p_conc_texto = ps[443]
p_intro_fim = ps[26]
p_ref_ultima = doc.paragraphs[-1]

# 1) capa: tira Marcos e Miguel
for i in (8, 6):
    ps[i]._p.getparent().remove(ps[i]._p)

# 2) dicionário de dados
celula(tabs[0], 4, 2, 'NOT NULL')
celula(tabs[0], 4, 3, 'Armazena a senha do usuário em formato HASH (SHA256). Não é única: duas pessoas podem ter a mesma senha.')
celula(tabs[1], 3, 2, 'NOT NULL')
t3 = tabs[3]
nova = copy.deepcopy(t3.rows[-1]._tr)
t3.rows[-1]._tr.addnext(nova)
celula(t3, 6, 0, 'ID_usuario'); celula(t3, 6, 1, 'INT')
celula(t3, 6, 2, 'FK (Usuario), NOT NULL'); celula(t3, 6, 3, 'Armazena e vincula o usuário que fez o pagamento.')
celula(t3, 3, 2, 'DEFAULT ‘PIX’, NOT NULL')
celula(t3, 5, 2, 'FK (Quadras), NOT NULL')
celula(tabs[4], 4, 2, 'DEFAULT ‘Pendente’, CHECK (Apenas ‘Pendente’, ‘Em andamento’, ‘Finalizado’ e ‘Cancelado’)')
celula(tabs[4], 4, 3, 'Armazena e verifica o status da reserva. ‘Cancelado’ libera o horário para outra pessoa.')

# 3) frontend
trocar(ps[98], 'Para Clientes:\n- Login e cadastro simples\n- Selecione esporte, quadra, data e horário\n- Pague o Pix (simulado) em até 5 minutos\n- Veja e cancele os seus agendamentos\n- Funciona em computadores, tablets e smartphones\n- Integração com VLibras para pessoas surdas')
trocar(ps[99], 'Para Administradores:\n- Visualize todos os agendamentos\n- Pesquise por nome ou data\n- Exclua agendamentos e usuários quando necessário\n- Baixe o relatório geral em TXT\n- Dados salvos no banco SQL Server\n\nPara Gestores:\n- Veja a agenda dos próximos 5 dias das suas quadras\n- Cancele agendamentos da sua quadra\n- Baixe o relatório das suas quadras em TXT, escolhendo o período (De / Até)')
trocar(ps[107], 'Preencha os dados solicitados:\n- Nome\n- Sobrenome\n- E-mail\n- CPF\n- Senha (mínimo 8 caracteres)\n- Confirmar Senha\n- Se for dono de quadra, marque a opção de gestor')
trocar(ps[110], 'Após criar sua conta, volte à tela de login e insira:\n- Seu E-mail (ele é o usuário do login)\n- Sua Senha')
trocar(ps[117], '- Faça login na sua conta\n- Abra a tela “Meus agendamentos”\n- Clique em “Cancelar” no agendamento que você não vai usar\n- O horário volta a ficar livre para outras pessoas\n- O gestor também cancela os agendamentos da quadra dele')
trocar(ps[123], '1. Na tela de Login, informe o e-mail de administrador (termina em @sunset.com) e a senha\n2. Clique em “Entrar”\n3. O sistema reconhece o perfil Admin e abre o Painel Administrativo\n(Esta forma de entrada mudou em relação ao protótipo, que tinha a caixa “Sou administrador”. Veja o Resumo dos tópicos alterados.)')

# 4) introdução
m = Montador(doc)
m.ancora = p_intro_fim._p.getnext()
m.texto('Neste semestre (PIM IV) o sistema foi modernizado. O protótipo em console, que gravava dados em arquivos de texto, virou uma solução em camadas com banco SQL Server, uma aplicação web (ASP.NET Core MVC), uma Web API e um aplicativo Flutter. As regras de negócio do semestre anterior foram mantidas. A seção 2.6 explica o que mudou e a seção 2.7 lista, tópico por tópico, o que era antes e o que ficou agora.')

m.ancora = p_conclusao._p
IMG = os.path.join(BASE, 'docs', 'img')

m.h2('2.6 Modernização para o PIM IV')
m.h3('2.6.1 O que mudou em relação ao protótipo')
m.texto('O protótipo do semestre anterior era um programa de console em C# com arquivos .txt, mais o módulo SistemaLogin que demonstrava herança, polimorfismo, validação de CPF e HASH SHA256. Ele serviu para provar as regras de negócio, mas não tinha banco de dados de verdade, telas web nem integração com outros sistemas.')
m.texto('Nesta versão o grupo manteve as regras e trocou a base técnica:')
m.lista(['Os arquivos .txt viraram um banco SQL Server (Express 2022) com as tabelas, triggers e a procedure que já estavam na documentação do banco.',
         'O console virou uma aplicação web (Sunset.Web) com login, perfis e telas de cliente, gestor e administrador.',
         'Foi criada uma Web API (Sunset.Api) com documentação OpenAPI, usada pelo aplicativo Flutter.',
         'O código foi separado em cinco projetos (camadas), seguindo o template do professor.',
         'A interface foi atualizada com Bootstrap, mantendo as telas, os campos e os nomes da documentação antiga.'])

m.h3('2.6.2 Arquitetura em camadas')
m.texto('A solution Sunset.sln tem cinco projetos. Cada um tem uma função e só conhece o projeto logo abaixo dele:')
m.lista(['Sunset.Modelos: entidades (Usuario, Quadra, Endereco, Pagamento, Agendamento, LogExclusaoQuadra), DTOs e constantes (perfis, status).',
         'Sunset.Repositorio: acesso ao banco. Usa Entity Framework Core nas consultas e cadastros e Dapper para chamar a procedure CadastrarAgendamento. Tem as migrations.',
         'Sunset.Negocio: regras de negócio (cadastro, login, CPF, senha com SHA256, reserva, cancelamento, Pix, relatório). Não sabe nada de telas.',
         'Sunset.Web: site MVC com Razor e Bootstrap. Os controllers só chamam a camada de Negócio.',
         'Sunset.Api: Web API REST. Os controllers também só chamam a camada de Negócio.'])
m.imagem(os.path.join(IMG, 'arquitetura.png'), 14.5, 'Figura – Arquitetura em camadas do SUNSET. Fonte: os autores (2026).')
m.texto('As referências entre os projetos seguem o sentido Web/Api → Negocio → Repositorio → Modelos. Os Program.cs da Web e da Api só registram os repositórios na injeção de dependência (AddScoped), exatamente como no template do professor. Os controllers não usam o repositório diretamente: eles recebem apenas as classes de Negócio no construtor.')
m.codigo('''// Trecho do Program.cs (Web e Api): injeção de dependência
builder.Services.AddDbContext<AppDbContext>(opcoes => opcoes.UseSqlServer(connectionString));

builder.Services.AddScoped<IQuadraRepositorio, QuadraRepositorio>();
builder.Services.AddScoped<IAgendamentoRepositorio, AgendamentoRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IPagamentoRepositorio, PagamentoRepositorio>();

builder.Services.AddScoped<QuadraNegocio>();
builder.Services.AddScoped<AgendamentoNegocio>();
builder.Services.AddScoped<UsuarioNegocio>();
builder.Services.AddScoped<RelatorioNegocio>();''', 'Código – Registro dos serviços. Fonte: os autores (2026).')
m.texto('A connection string fica somente no appsettings.json (Server=localhost\\SQLEXPRESS;Database=Sunset;Trusted_Connection=True;TrustServerCertificate=True). Nenhum arquivo de código tem usuário, senha ou servidor escrito.')

m.h3('2.6.3 Tecnologias usadas')
m.tabela(['Parte', 'Tecnologia', 'Para que serve', 'Onde está'],
 [['Linguagem', 'C# com .NET 9', 'Todo o back-end e o site', 'Pastas Sunset.*'],
  ['Banco', 'SQL Server 2022 Express', 'Guardar usuários, quadras, pagamentos e agendamentos', 'banco/ e Migrations'],
  ['Acesso a dados', 'Entity Framework Core 9', 'Consultas, cadastros e migrations', 'Sunset.Repositorio'],
  ['Acesso a dados', 'Dapper', 'Chamar a procedure CadastrarAgendamento', 'AgendamentoRepositorio.cs'],
  ['Site', 'ASP.NET Core MVC, Razor e Bootstrap', 'Telas responsivas do cliente, gestor e admin', 'Sunset.Web'],
  ['API', 'ASP.NET Core Web API, OpenAPI e Scalar', 'Endpoints para o aplicativo e documentação em /scalar', 'Sunset.Api'],
  ['Aplicativo', 'Flutter (Dart) e pacote http', 'App que consome a Web API', 'sunset_app/'],
  ['Acessibilidade', 'VLibras', 'Tradução para LIBRAS no site', '_Layout.cshtml']],
 'Tabela – Tecnologias do projeto. Fonte: os autores (2026).')

m.h3('2.6.4 Banco de dados com EF Core, Dapper e procedure')
m.texto('O modelo das tabelas continua o da documentação do banco (seção 2.1). O AppDbContext descreve as tabelas, as restrições (CHECK, UNIQUE) e as chaves estrangeiras. A migration Inicial cria tudo no SQL Server e também cria, com SQL escrito à mão, as três triggers (trg_LogExclusaoQuadras, trg_VirarGestor e trg_ExclusaoSomenteAdmin) e a procedure CadastrarAgendamento. As tabelas que têm trigger usam HasTrigger no AppDbContext, porque o EF Core precisa saber disso para salvar corretamente.')
m.texto('As chaves estrangeiras ficaram sem exclusão em cascata (Restrict). Por isso o banco recusa excluir uma quadra ou um usuário que ainda tem agendamentos ou pagamentos, e o sistema mostra uma mensagem simples para o usuário.')
m.texto('O agendamento é feito pela procedure, chamada com o Dapper. Os valores vão como parâmetros e não dentro do texto do comando, o que evita SQL Injection. Se a procedure lançar um erro (RAISERROR, número 50000), o repositório transforma em uma exceção que a camada de Negócio entende.')
m.codigo('''public int CadastrarPelaProcedure(DateTime data, TimeSpan horaInicio, TimeSpan horaFim,
                                  int usuarioId, int quadraId, int pagamentoId)
{
    // Reaproveita a mesma conexão (e a mesma connection string) do EF.
    var conexao = _contexto.Database.GetDbConnection();

    var parametros = new DynamicParameters();
    parametros.Add("DataReserva", data.Date, DbType.Date);
    parametros.Add("HoraInicio", horaInicio, DbType.Time);
    parametros.Add("HoraFim", horaFim, DbType.Time);
    parametros.Add("ID_Usuario", usuarioId);
    parametros.Add("ID_Quadra", quadraId);
    parametros.Add("ID_Pagamento", pagamentoId);

    try
    {
        return conexao.QuerySingle<int>("CadastrarAgendamento", parametros,
                                        commandType: CommandType.StoredProcedure);
    }
    catch (SqlException ex) when (ex.Number == 50000)
    {
        throw new ErroDeBancoException(ex.Message);
    }
}''', 'Código – Chamada da procedure com Dapper (AgendamentoRepositorio.cs). Fonte: os autores (2026).')
m.texto('A procedure usa transação com UPDLOCK e HOLDLOCK ao conferir se o horário está livre. Assim, duas pessoas que tentam o mesmo horário ao mesmo tempo não conseguem reservar os dois. Agendamentos cancelados são ignorados na conferência, então o horário volta a ficar livre.')
m.texto('A trigger trg_ExclusaoSomenteAdmin precisa saber quem está excluindo. Antes do DELETE, o repositório grava o id de quem executa na sessão do SQL Server (sp_set_session_context, chave ExecutorID). A trigger lê esse valor com SESSION_CONTEXT e só deixa excluir se a pessoa for Admin.')
m.codigo('''// UsuarioRepositorio.Excluir: define o ExecutorID e exclui na mesma chamada
_contexto.Database.ExecuteSqlInterpolated(
    $"EXEC sp_set_session_context N'ExecutorID', {executorId}; DELETE FROM Usuario WHERE IDUsuario = {usuarioId}");''', 'Código – Exclusão de usuário com ExecutorID. Fonte: os autores (2026).')

m.h3('2.6.5 Camada de Negócio')
m.texto('É a camada que guarda as regras do sistema. Os principais pontos são:')
m.lista(['Cadastro: exige nome, sobrenome, e-mail, CPF e senha. O CPF é conferido pelos dois dígitos verificadores (classe Validacao). A senha precisa ter no mínimo 8 caracteres e é guardada em SHA256 (classe Seguranca).',
         'Login: feito pelo e-mail. A senha digitada passa pelo mesmo SHA256 e é comparada com o hash do banco.',
         'Perfis: Admin (e-mail @sunset.com), Gestor (dono de quadra) e Cliente.',
         'Reserva: horários de 1 hora, das 08h às 22h, até 60 dias à frente. Não aceita horário que já passou.',
         'Pix: a reserva nasce Pendente com um pagamento Pendente. Quem não paga em 5 minutos perde o horário (o servidor confere o prazo). Ao pagar, a reserva vira Em andamento.',
         'Cancelamento: o cliente cancela a própria reserva e o gestor cancela as da quadra dele.',
         'Exclusões: só o Admin exclui usuários e agendamentos.'])
m.codigo('''public ReservaCriadaDto Reservar(int usuarioId, int quadraId, DateTime data, TimeSpan horaInicio)
{
    ExpirarPagamentosVencidos();   // libera horários de quem não pagou a tempo

    var quadra = _quadras.ObterPorId(quadraId)
        ?? throw new RegraDeNegocioException("Quadra não encontrada.");

    if (data + horaInicio <= DateTime.Now)
        throw new RegraDeNegocioException("Esse horário já passou. Escolha um horário futuro.");

    var pagamento = new Pagamento { Valor = quadra.PrecoHora, MetodoPagamento = "PIX",
                                    StatusPagamento = StatusPagamento.Pendente,
                                    QuadraId = quadraId, UsuarioId = usuarioId };
    _pagamentos.Adicionar(pagamento);

    try
    {
        var id = Agendar(data, horaInicio, horaInicio.Add(TimeSpan.FromHours(1)),
                         usuarioId, quadraId, pagamento.Id);
        return new ReservaCriadaDto { AgendamentoId = id, PagamentoId = pagamento.Id };
    }
    catch (RegraDeNegocioException)
    {
        _pagamentos.Remover(pagamento);   // não deu certo: apaga o pagamento criado à toa
        throw;
    }
}''', 'Código – Reserva (AgendamentoNegocio.cs, resumido). Fonte: os autores (2026).')

m.h3('2.6.6 Aplicação web (MVC)')
m.texto('O site usa ASP.NET Core MVC com Razor. O login é feito com cookie: depois de entrar, o navegador guarda o id, o nome, o e-mail e o perfil do usuário. Os controllers usam [Authorize] e [Authorize(Roles = ...)] para barrar quem não tem permissão. Os formulários usam token anti-falsificação (ValidateAntiForgeryToken).')
m.lista(['Conta: login e cadastro.',
         'Reservas: escolher o esporte, a quadra, a data e o horário; Meus agendamentos; agenda do gestor (5 dias).',
         'Pagamentos: tela do Pix com QR de exemplo, cronômetro de 5 minutos e botão “Simular pagamento”.',
         'Quadras: o gestor cadastra, edita e exclui as quadras dele.',
         'Admin: lista de usuários, lista de agendamentos com filtro por nome e data, exclusões e o botão do relatório TXT.'])
m.codigo('''// ReservasController.cs: o controller só conversa com a camada de Negócio
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Reservar(int quadraId, DateTime data, TimeSpan hora)
{
    try
    {
        var reserva = _negocio.Reservar(User.Id(), quadraId, data, hora);
        return RedirectToAction("Pix", "Pagamentos", new { id = reserva.PagamentoId });
    }
    catch (RegraDeNegocioException ex)
    {
        TempData["Erro"] = ex.Message;
        return RedirectToAction(nameof(Horarios), new { id = quadraId, data = data.ToString("yyyy-MM-dd") });
    }
}''', 'Código – Action de reserva no site (ReservasController.cs). Fonte: os autores (2026).')
m.texto('O visual usa Bootstrap com a cor roxa da documentação antiga, e a tela se ajusta a computador, tablet e celular. O widget do VLibras fica no canto inferior direito, como descrito na seção 2.2.9.')
m.marcador('[PRINT 1 – Tela de login do site]')
m.marcador('[PRINT 2 – Tela de horários da quadra]')
m.marcador('[PRINT 3 – Tela do Pix]')
m.marcador('[PRINT 4 – Painel do administrador]')

m.h3('2.6.7 Web API')
m.texto('A Web API devolve JSON e usa os mesmos serviços de Negócio do site. A documentação interativa (OpenAPI com Scalar) fica em /scalar quando a Api está rodando. O CORS está liberado para o aplicativo conseguir chamar a Api quando roda no navegador.')
m.tabela(['Método', 'Rota', 'O que faz', 'Retorno'],
 [['POST', 'api/usuarios', 'Cadastra um usuário', '201 ou 400 com mensagem'],
  ['POST', 'api/usuarios/login', 'Login por e-mail e senha', '200 com o usuário ou 401'],
  ['GET', 'api/quadras', 'Lista as quadras', '200'],
  ['GET', 'api/quadras/{id}', 'Mostra uma quadra', '200 ou 404'],
  ['GET', 'api/quadras/{id}/horarios?data=', 'Horários livres e ocupados do dia', '200'],
  ['POST', 'api/agendamentos/reservar', 'Cria o pagamento pendente e agenda 1 hora', '201 ou 400'],
  ['GET', 'api/agendamentos?usuarioId=', 'Agendamentos do cliente', '200'],
  ['POST', 'api/agendamentos/{id}/cancelar', 'Cancela a reserva', '200 ou 400'],
  ['GET', 'api/pagamentos/{id}/pix?usuarioId=', 'Dados do Pix e tempo restante', '200 ou 400'],
  ['POST', 'api/pagamentos/{id}/simular', 'Simula o pagamento do Pix', '200 ou 400']],
 'Tabela – Principais endpoints da Api. Fonte: os autores (2026).')
m.texto('Limitação assumida: a Api não usa token (JWT). O app envia o id do usuário nas chamadas. Isso foi aceito para manter o projeto simples; em um sistema real seria preciso autenticar cada chamada.')

m.h3('2.6.8 Aplicativo Flutter')
m.texto('O aplicativo fica na pasta sunset_app e usa o pacote http. A classe ServicoApi concentra todas as chamadas à Web API. A url base é http://localhost:5218 no navegador e http://10.0.2.2:5218 no emulador Android. As telas são: login, quadras, horários, Pix e meus agendamentos, o mesmo caminho do cliente no site.')
m.marcador('[PRINT 5 – Telas do aplicativo Flutter]')

m.h3('2.6.9 Fluxo de uma reserva')
m.imagem(os.path.join(IMG, 'fluxo_reserva.png'), 15.5, 'Figura – Fluxo de uma reserva. Fonte: os autores (2026).')
m.texto('Resumindo: o cliente escolhe o horário, o controller chama o Negócio, o Negócio cria um pagamento Pendente e pede para o repositório executar a procedure. Se o horário estiver livre, a reserva fica Pendente e o cliente tem 5 minutos para pagar o Pix. Pagando, a reserva vira Em andamento. O status Finalizado é calculado quando o horário termina.')

m.h3('2.6.10 Análise de dados e relatório')
m.texto('O manual do PIM cita uma etapa de Machine Learning e análise de dados. O grupo decidiu não implementar Machine Learning neste semestre, para entregar o restante bem testado. Como análise de dados, o administrador baixa um relatório geral em TXT com total de usuários por perfil, quadras por esporte, agendamentos por status, faturamento (soma dos pagamentos aprovados) e a lista de agendamentos. O gestor, que pela documentação do semestre anterior também gera relatórios (critério CA2), baixa na tela da agenda o relatório das quadras dele, em TXT, com os agendamentos por status, o faturamento e a lista, podendo escolher o período (De / Até). O relatório do gestor usa o id de quem está logado, então ele nunca recebe dados de quadras de outros gestores. O código está em RelatorioNegocio.cs (métodos Gerar e GerarDoGestor) e em ReservasController.cs (rota Relatorio).')

m.h3('2.6.11 Acessibilidade, comunicação e trabalho em equipe')
m.texto('A acessibilidade em LIBRAS é feita com o VLibras, presente em todas as páginas do site.')
m.marcador('[GRUPO: escrever aqui, em poucas linhas, como a equipe se comunicou, dividiu as tarefas e resolveu desacordos (Etapa 8 do manual).]')

m.h3('2.6.12 Como instalar e rodar')
m.texto('Pré-requisitos: .NET SDK (o projeto usa net9.0), SQL Server Express com a instância SQLEXPRESS, Git e, para o aplicativo, o Flutter. Cada pasta de entrega tem o seu README com os comandos; o resumo da versão final (pasta 05-flutter-final) é este:')
m.codigo('''1) Criar o banco (na pasta da solution):
   dotnet restore
   dotnet ef database update --project Sunset.Repositorio --startup-project Sunset.Api

2) (Opcional) Dados de exemplo e testes, no SSMS ou Azure Data Studio:
   banco/dados_exemplo.sql
   banco/testes.sql      (rodar só com o banco limpo)

3) Subir a Api (porta 5218):
   dotnet run --project Sunset.Api
   Documentação interativa: http://localhost:5218/scalar

4) Subir o site (porta 5157):
   dotnet run --project Sunset.Web

5) Aplicativo Flutter (com a Api rodando):
   cd sunset_app
   flutter pub get
   flutter run -d web-server --web-port 8123''', 'Código – Passo a passo para rodar. Fonte: os autores (2026).')
m.texto('Logins para teste: admin@sunset.com com a senha Admin@1234 (Admin) e, depois de rodar dados_exemplo.sql, amanda.r@gmail.com, bruno.os@outlook.com e mariana.costa@hotmail.com com a senha Senha1234.')

m.h3('2.6.13 Testes realizados')
m.tabela(['O que foi testado', 'Como', 'Resultado', 'Situação'],
 [['Banco: agendamento, conflito, horário inválido, vínculo inexistente, cancelamento', 'banco/testes.sql no SQL Server Express (testes 1 a 6)', 'Resultados esperados conferidos', 'Passou'],
  ['Banco: CPF repetido, e-mail inválido, exclusão sem Admin e como Admin, nome de quadra com caractere especial, log e trigger de gestor', 'banco/testes.sql (testes 7 a 13)', 'Resultados esperados conferidos', 'Passou'],
  ['Regras de negócio', 'Programa de teste local com repositórios de mentira', 'Cadastro, CPF, senha, login, reserva, conflito, cancelamento, Pix e expiração', 'Passou'],
  ['Api', 'Chamadas reais contra o SQL Server', 'Login, reserva, Pix e cancelamento', 'Passou'],
  ['Site', 'Navegador, perfis Cliente, Gestor e Admin', 'Telas e fluxos principais', 'Passou'],
  ['Relatório do gestor', 'Programa de teste local com repositórios de mentira', 'Gestor só recebe as quadras dele, filtro de período, Admin vê todas, gestor sem quadra recebe relatório vazio', 'Passou (botão de baixar o TXT e relatório do gestor conferidos na tela em 21/10/2026, no Codespaces)'],
  ['Aplicativo Flutter', 'Chrome contra a Api real; flutter analyze e flutter test', 'Fluxo completo; 1 erro de setState achado e corrigido', 'Passou (Android não testado)']],
 'Tabela – Testes. Fonte: os autores (2026).')
m.marcador('[PRINT 6 – Resultado do banco/testes.sql no SSMS]')

m.h3('2.6.14 Decisões técnicas')
m.lista(['Camadas e injeção de dependência como no template do professor, para a Web e a Api reaproveitarem a mesma regra de negócio.',
         'EF Core para o dia a dia e Dapper só para a procedure, que é o que o PIM pede.',
         'SHA256 sem sal, mantido porque a documentação antiga define SHA256. Por isso a coluna Senha deixou de ser única.',
         'Chaves estrangeiras sem cascata, para o banco nunca apagar registros em cadeia sem querer.',
         'Pix simulado, porque integrar com um banco de verdade fugiria do escopo.',
         'Sem Machine Learning, por decisão do grupo.'])

m.h3('2.6.15 Limitações e pendências')
m.lista(['A Api não tem token; o app manda o id do usuário (seção 2.6.7).',
         'O Pix é simulado e o QR é uma imagem de exemplo.',
         'Cancelar uma reserva já paga não faz estorno: o pagamento continua como Aprovado. Cancelar uma reserva pendente deixa o pagamento como Cancelado.',
         'Na agenda do gestor, horários de hoje que já passaram aparecem como Livre.',
         'Não há Machine Learning (decisão do grupo); a análise de dados é o relatório TXT.',
         'O aplicativo não foi testado no Android (sem Android SDK no computador usado); foi testado no navegador.',
         'banco/testes.sql não pode ser rodado duas vezes seguidas: precisa de um banco limpo.',
         'A documentação antiga tem mais páginas que o limite de 20 a 30 páginas do manual; o grupo deve decidir o que enxugar.'])

m.h2('2.7 Resumo dos tópicos alterados')
m.texto('A tabela mostra, para cada tópico, como era na documentação ou no protótipo, como ficou agora e por quê.')
m.tabela(['Tópico', 'Antes', 'Agora', 'Por quê'],
 [['Armazenamento', 'Arquivos .txt', 'SQL Server com EF Core e Dapper', 'Exigência do PIM e dados mais seguros'],
  ['Estrutura do código', 'Dois módulos de console', 'Cinco projetos em camadas', 'Template do professor; Web e Api reaproveitam o Negócio'],
  ['Versão web e app', 'Não existia (só a documentação das telas)', 'Site MVC, Web API e app Flutter', 'Exigência do PIM'],
  ['Usuario.Senha', 'NOT NULL, UNIQUE', 'NOT NULL, sem UNIQUE', 'Com hash sem sal, uma restrição única revelaria senhas iguais'],
  ['Pagamentos.ID_usuario', 'Não estava no dicionário', 'Coluna NOT NULL com FK', 'O INSERT antigo falharia sem essa coluna'],
  ['Status do agendamento', 'Pendente, Em andamento, Finalizado', 'Inclui Cancelado', 'O cliente passou a cancelar; o horário volta a ficar livre'],
  ['Cancelamento', 'Só o administrador, por contato', 'Cliente (e gestor da quadra) cancelam na tela', 'Decisão do grupo'],
  ['PrecoHora e quadra no pagamento', 'Aceitavam vazio', 'NOT NULL', 'Evitar reserva sem preço ou sem quadra'],
  ['trg_ExclusaoSomenteAdmin', 'Usava ROLLBACK', 'Só RAISERROR e RETURN (INSTEAD OF DELETE)', 'ROLLBACK dentro da INSTEAD OF causava erro; ExecutorID vem do repositório'],
  ['Procedure CadastrarAgendamento', 'Mostrava mensagem com PRINT', 'Devolve o id com SELECT; usa transação e UPDLOCK/HOLDLOCK', 'O sistema precisa do id e do bloqueio contra reserva dupla'],
  ['Chaves estrangeiras', 'Sem regra definida', 'Sem ON DELETE CASCADE (Restrict)', 'O SQL Server recusou a cascata em Agendamentos'],
  ['CPF', 'Só 11 números', 'Também confere os dígitos verificadores', 'Regra de negócio mais completa'],
  ['Textos gravados', 'Livres', 'Sem acento antes de gravar quadra, cidade etc.', 'Os CHECKs do banco só aceitam letras sem acento, números e espaços'],
  ['Login', 'Campo “Usuário”', 'Usuário é o e-mail', 'Decisão do grupo; e-mail já é único no banco'],
  ['Entrada do administrador', 'Caixa “Sou administrador” na tela de login', 'Login único; o perfil Admin é reconhecido pelo e-mail @sunset.com', 'Mais simples; o grupo deve confirmar se quer a caixa de volta'],
  ['Pagamento', 'Prazo de pagamento no protótipo', 'Pix simulado com 5 minutos conferidos no servidor', 'Decisão do grupo; sem integração real'],
  ['Horários', 'Lista fixa de horários', 'De 1 hora, 08h às 22h, até 60 dias à frente', 'Regra de negócio única para site e app'],
  ['Machine Learning', 'Não havia', 'Não implementado; relatório TXT', 'Decisão do grupo'],
  ['Relatório', 'O fluxo 2.0.2 falava em “Exportar PDF” e o RNF02 pedia TXT; o protótipo só imprimia o resultado do SELECT', 'Relatório em TXT: o Admin baixa o geral e o gestor baixa o das quadras dele, com filtro de período', 'O RNF02 vale (TXT) e o CA2 dá relatórios ao gestor; o PDF não foi feito'],
  ['Visual do site', 'Telas simples da documentação', 'Bootstrap roxo e responsivo, mesmos campos e nomes', 'Repaginada pedida no PIM']],
 'Tabela – Resumo dos tópicos alterados. Fonte: os autores (2026).')

# conclusão: acrescenta um parágrafo
m.ancora = p_conc_texto._p.getnext()
m.texto('Nesta etapa do PIM, o SUNSET passou de protótipo para um sistema com banco de dados, site, Web API e aplicativo. As regras de negócio foram preservadas e testadas, e as limitações que ficaram (Api sem token, Pix simulado, sem Machine Learning) estão descritas na seção 2.6.15.')

# referências
ult = p_ref_ultima._p
m.ancora = ult.getnext()
if m.ancora is None:
    sp = OxmlElement('w:p'); ult.addnext(sp); m.ancora = sp
for r in ['MICROSOFT. Entity Framework Core. Disponível em: <https://learn.microsoft.com/ef/core/>. Acesso em: 18 out. 2026.',
          'MICROSOFT. ASP.NET Core MVC. Disponível em: <https://learn.microsoft.com/aspnet/core/mvc/overview>. Acesso em: 18 out. 2026.',
          'DAPPER. Dapper – a simple object mapper for .NET. Disponível em: <https://github.com/DapperLib/Dapper>. Acesso em: 18 out. 2026.',
          'FLUTTER. Flutter documentation. Disponível em: <https://docs.flutter.dev>. Acesso em: 18 out. 2026.',
          'BRASIL. VLibras. Disponível em: <https://vlibras.gov.br>. Acesso em: 18 out. 2026.']:
    m.texto(r)

# sumário: o Word atualiza ao abrir
upd = OxmlElement('w:updateFields'); upd.set(qn('w:val'), 'true')
doc.settings.element.append(upd)

os.makedirs(os.path.join(BASE, 'docs'), exist_ok=True)
saida = os.path.join(BASE, 'docs', 'DOCUMENTACAO_SUNSET.docx')
doc.save(saida)
print('salvo em', saida)
