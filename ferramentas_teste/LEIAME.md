# Ferramentas de teste (não fazem parte da entrega)

- `TesteRegrasNegocio_Program.cs` + `h.csproj`: teste local sem banco das regras de negócio (usa repositórios falsos). Era um projeto console que referenciava Sunset.Negocio, Sunset.Repositorio e Sunset.Modelos da Pasta 4. Testou cadastro, login, CPF, reservas, cancelamento, Pix, expiração, exclusões do Admin e relatório.
- `mock_api.py`: Api de mentira (porta 5218) que devolve o mesmo JSON da Api real, usada só para testar o app Flutter no navegador.
- `teste.py` e `run.sh`: roteiro Playwright que abriu o app Flutter web (build com `--no-web-resources-cdn`), fez login, reservou, pagou o Pix e cancelou.
