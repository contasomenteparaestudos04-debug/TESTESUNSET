# Scripts de População e Testes do Banco de Dados

Este documento contém os scripts SQL para popular as tabelas do banco de dados e viriações de cenários para validação das regras de negócio e restrições implementadas.

---

## Sumário

- [1. Populando a Tabela: Usuario](#1-populando-a-tabela-usuario)
- [2. Populando a Tabela: Enderecos](#2-populando-a-tabela-enderecos)
- [3. Populando a Tabela: Quadras](#3-populando-a-tabela-quadras)
- [4. Populando a Tabela: Pagamentos](#4-populando-a-tabela-pagamentos)
- [5. Testes de Agendamento](#5-testes-de-agendamento)
- [6. Testes de Restrições de Integridade](#6-testes-de-restrições-de-integridade)

---

## 1. Populando a Tabela: `Usuario`

```sql
INSERT INTO Usuario (NomeSobrenome, Email, Senha, CPF, Permissao, Genero, StatusUsuario) VALUES
('Carlos Henrique Silva', 'carlos.silva@email.com', '$2a$12$Nw75zcGOkeQ.biVGWHfte.MV/Gvv/WpHtTgy4HcQtPrhJ/p979Cbu', '12345678901', 'Admin',   'M', 'Ativo'),
('Amanda Rodrigues',      'amanda.r@gmail.com',     '$2a$12$lwDgMaThgWekoCvaeThr.eJvt1a52zi01PMP9YgDgiNA5gKBYYZLC', '98765432100', 'Cliente', 'F', 'Ativo'),
('Bruno Oliveira Santos', 'bruno.os@outlook.com',   '$2a$12$JMCvLoEvOpbRdZhOrBm2sOrtDYW7DsVSTl8mEpxCZ54Yz9IWVOi7u', '45678912311', 'Cliente', 'M', 'Ativo'),
('Mariana Costa',         'mariana.costa@hotmail.com', '$2a$12$/OH5wk2dkHwUrxMJ06jFuuqP7mwJ1yecds0D0mqb4JntWOz2Swpou', '74185296300', 'Gestor', 'F', 'Ativo');
GO
```

---

## 2. Populando a Tabela: `Enderecos`

```sql
INSERT INTO Enderecos (CEP, Logradouro, Numero, Bairro, Cidade, Estado) VALUES
('14020250', 'Avenida João Fiúsa', '1500', 'Jardim Botânico', 'Ribeirão Preto', 'SP'),
('14050000', 'Rua São Sebastião',  '342',  'Centro',          'Ribeirão Preto', 'SP');
GO
```

---

## 3. Populando a Tabela: `Quadras`

> **Regra de negócio:** Se o usuário associado em `ID_gestor` possuir permissão `'Cliente'`, ele será promovido automaticamente para `'Gestor'`.

```sql
INSERT INTO Quadras (NomeQuadra, PrecoHora, ID_gestor) VALUES
('Arena Sunset - Society 1', 150.00, 2), -- Amanda (ID 2) vira Gestora automaticamente
('Quadra de Beach Tennis A',  90.00, 4);
GO
```

---

## 4. Populando a Tabela: `Pagamentos`

```sql
INSERT INTO Pagamentos (ValorTotal, StatusPagamento, DataPagamento, ID_Quadra) VALUES
(150.00, 'Aprovado', GETDATE(), 1),
( 90.00, 'Pendente', GETDATE(), 2);
GO
```

---

## 5. Testes de Agendamento

### Teste 1 — Agendamento bem-sucedido

```sql
EXEC CadastrarAgendamento
    @DataReserva  = '2026-06-15',
    @HoraInicio   = '18:00:00',
    @HoraFim      = '19:00:00',
    @ID_Usuario   = 3,  -- Bruno
    @ID_Quadra    = 1,  -- Arena Sunset - Society 1
    @ID_Pagamento = 1;
GO
```

---

### Teste 2 — Agendamento bem-sucedido em horários diferentes na mesma quadra

```sql
-- Horário imediatamente posterior ao Teste 1, sem sobreposição
EXEC CadastrarAgendamento
    @DataReserva  = '2026-06-15',
    @HoraInicio   = '19:00:00',
    @HoraFim      = '20:00:00',
    @ID_Usuario   = 2,  -- Amanda
    @ID_Quadra    = 1,  -- Arena Sunset - Society 1
    @ID_Pagamento = 2;
GO
```

---

### Teste 3 — Tentativa de agendamento em conflito de horário

> **Erro esperado:** `"Erro: Esta quadra já possui um agendamento para o horário selecionado."`

O intervalo `18:30–19:30` sobrepõe o agendamento do Teste 1 (`18:00–19:00`), ativando a verificação de conflito na procedure.

```sql
EXEC CadastrarAgendamento
    @DataReserva  = '2026-06-15',
    @HoraInicio   = '18:30:00',
    @HoraFim      = '19:30:00',
    @ID_Usuario   = 2,
    @ID_Quadra    = 1,
    @ID_Pagamento = 3;
GO
```

---

### Teste 4 — Horário de término menor ou igual ao de início

> **Erro esperado:** `"Erro: O horário de término deve ser maior que o horário de início."`

```sql
EXEC CadastrarAgendamento
    @DataReserva  = '2026-06-15',
    @HoraInicio   = '20:00:00',
    @HoraFim      = '19:00:00',  -- HoraFim < HoraInicio
    @ID_Usuario   = 3,
    @ID_Quadra    = 2,  -- Quadra de Beach Tennis A
    @ID_Pagamento = 2;
GO
```

---

### Teste 5 — Vínculo inválido (FK inexistente)

> **Erro esperado:** `"Erro de vínculo: O Usuário, Quadra ou Pagamento informado não existe."`

O CATCH da procedure captura o erro `547` (violação de chave estrangeira) e retorna o erro tratado.

```sql
EXEC CadastrarAgendamento
    @DataReserva  = '2026-06-15',
    @HoraInicio   = '21:00:00',
    @HoraFim      = '22:00:00',
    @ID_Usuario   = 999,  -- Usuário inexistente
    @ID_Quadra    = 1,
    @ID_Pagamento = 1;
GO
```

---

## 6. Testes de Restrições de Integridade

### Teste 6 — CPF com padrão de dígitos repetidos

> **Restrição disparada:** `chk_cpf_repetido`
> O banco bloqueia imediatamente o cadastro de CPFs inválidos com todos os dígitos iguais.

```sql
INSERT INTO Usuario (NomeSobrenome, Email, Senha, CPF, Permissao, Genero, StatusUsuario)
VALUES ('Testador Falha', 'falha.cpf@email.com', 'SenhaSecure123', '22222222222', 'Cliente', 'M', 'Ativo');
GO
```

---

### Teste 7 — E-mail sem formato válido (sem `@`)

> **Restrição disparada:** `chk_email_formato`

```sql
INSERT INTO Usuario (NomeSobrenome, Email, Senha, CPF, Permissao, Genero, StatusUsuario)
VALUES ('Erro Email', 'usuario_sem_email.com', 'Senha123', '96385274100', 'Cliente', 'M', 'Ativo');
GO
```

---

### Teste 8 — Exclusão de usuário sem permissão de Admin

> **Comportamento esperado:** Se o usuário não estiver autenticado como `Admin`, a transação sofre **ROLLBACK** com o erro de permissão disparado.

```sql
DELETE FROM Usuario WHERE IDUsuario = 3;
GO
```
