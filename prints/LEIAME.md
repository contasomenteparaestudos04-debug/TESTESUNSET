# Prints do sistema SUNSET

- `desktop/`: site em 1280px de largura. `celular/`: site em 390px (tela de celular). Cada tela foi tirada como página inteira.
- `app/`: app Flutter rodando no Chrome, em 390x844.

## Como foram feitos

Os prints foram tirados num ambiente sem SQL Server. O site e a Api rodaram com os mesmos controllers, views e regras de negocio, mas com repositorios em memoria (dados de exemplo) no lugar do banco. Nada das pastas de entrega foi alterado. O banco real, a procedure e as triggers nao foram exercitados nesses prints.

A tela de relatorio do gestor nao tem print: ela baixa um arquivo TXT, nao abre uma pagina.

## Atualizacao

Os prints de `desktop/` e `celular/` foram refeitos depois do ajuste de responsividade: no celular (ate 767px) as tabelas de Meus agendamentos, Minhas quadras, Admin > Usuarios e Admin > Agendamentos viram cartoes empilhados, sem precisar rolar a tabela para o lado. Em tela larga nada mudou.
