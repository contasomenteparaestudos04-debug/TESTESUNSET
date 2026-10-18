using Microsoft.EntityFrameworkCore;
using Sunset.Negocio;
using Sunset.Repositorio;
using Sunset.Repositorio.Contexto;
using Sunset.Repositorio.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// A connection string vem do appsettings.json, nunca do código-fonte.
var connectionString = builder.Configuration.GetConnectionString("Padrao")
    ?? throw new InvalidOperationException("Connection string 'Padrao' não encontrada no appsettings.json.");

// ---------- Injeção de dependência ----------
// Mesmo bloco do Program.cs da Api (ver explicação lá e no INJECAO-DE-DEPENDENCIA.md).

// 1) O banco
builder.Services.AddDbContext<AppDbContext>(opcoes => opcoes.UseSqlServer(connectionString));

// 2) Repositórios (interface -> classe)
builder.Services.AddScoped<IQuadraRepositorio, QuadraRepositorio>();
builder.Services.AddScoped<IAgendamentoRepositorio, AgendamentoRepositorio>();

// 3) Negócio
builder.Services.AddScoped<QuadraNegocio>();
builder.Services.AddScoped<AgendamentoNegocio>();
// --------------------------------------------

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Idioma pt-BR: números com vírgula (150,00) e moeda em R$, em qualquer máquina.
app.UseRequestLocalization("pt-BR");

app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
