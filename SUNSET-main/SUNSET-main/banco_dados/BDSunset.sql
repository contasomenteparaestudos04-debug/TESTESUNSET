CREATE DATABASE SUNSET;
GO

USE SUNSET;
GO

CREATE TABLE Usuario (
	IDUsuario INT PRIMARY KEY IDENTITY(1,1), --Identity para autoincremento.
	NomeSobrenome VARCHAR(100) NOT NULL CHECK (NomeSobrenome NOT LIKE '%[^a-zA-Z0-9 ]%'),
	Email VARCHAR (100) NOT NULL UNIQUE, 
	Senha VARCHAR (255) NOT NULL UNIQUE, --Numero de caracteres alto para criptografia.
	CPF CHAR(11) NOT NULL UNIQUE,
	Permissao VARCHAR(20) CHECK (Permissao IN('Admin', 'Gestor', 'Cliente')),
	Genero CHAR(1) CHECK (Genero IN('M', 'F')),
	StatusUsuario VARCHAR(15) CHECK (StatusUsuario IN('Inativo', 'Ativo', 'Bloqueado')),
	CONSTRAINT CHK_email_formato CHECK (Email LIKE '%_@__%.__%'), --Garante pelo menos um caractere antes do @, dois caracteres depois do @ e antes do ponto e dois caracteres depois do ponto.
	CONSTRAINT CHK_cpf_repetido CHECK (CPF NOT LIKE REPLICATE(SUBSTRING(CPF, 1, 1), 11)), --Verifica se o CPF não é apenas um número repetido com base no primeiro digito.
	CONSTRAINT CHK_definir_admin CHECK ((Permissao = 'Admin' AND Email LIKE '%@sunset.com') OR (Permissao IN ('Gestor', 'Cliente'))),
	CONSTRAINT CHK_cpf_apenas_numeros CHECK (CPF NOT LIKE '%[^0-9]%')
);
GO

CREATE TABLE Enderecos (
	IDEndereco INT PRIMARY KEY IDENTITY(1,1),
	CEP VARCHAR(8) NOT NULL,
	Logradouro VARCHAR(70) NOT NULL CHECK (Logradouro NOT LIKE '%[^a-zA-Z0-9 ]%'),
	Numero VARCHAR(5),
	Bairro VARCHAR(40),
	Cidade VARCHAR(40) NOT NULL CHECK (Cidade NOT LIKE '%[^a-zA-Z0-9 ]%'),
	Estado VARCHAR(40) NOT NULL CHECK (Estado NOT LIKE '%[^a-zA-Z0-9 ]%')
);
GO

CREATE TABLE Quadras (
	IDQuadras INT PRIMARY KEY IDENTITY(1,1), --Identity para autoincremento.
	NomeQuadra VARCHAR(50) NOT NULL CHECK (NomeQuadra NOT LIKE '%[^a-zA-Z0-9 ]%'),
	PrecoHora DECIMAL(8,2),
	Categoria VARCHAR(40) CHECK(Categoria IN('Volei', 'Beach Tennis')),
	ID_endereco INT,
	ID_gestor INT,
	CONSTRAINT FK_Quadras_Enderecos FOREIGN KEY (ID_endereco)
	REFERENCES Enderecos(IDEndereco),
	CONSTRAINT FK_Quadra_Usuario FOREIGN KEY (ID_gestor)
	REFERENCES Usuario(IDUsuario)
);
GO

CREATE TABLE Pagamentos (
	IDPagamento INT PRIMARY KEY IDENTITY(1,1),
	Valor DECIMAL(10,2),
	DataPagamento DATETIME,
	MetodoPagamento VARCHAR(20) DEFAULT 'PIX',
	StatusPagamento VARCHAR(20) NOT NULL 
	CONSTRAINT CHK_Status_Pagamento --Cria uma função com o nome CHK_Status_Pagamento
	--Define o valor padrão para 'Pendente' por precaução.
	DEFAULT 'Pendente',
	CHECK (StatusPagamento IN ('Pendente', 'Aprovado', 'Cancelado')), --A função checa qual o status do pagamento.

	ID_quadra INT,
	CONSTRAINT FK_Quadras_Pagamentos FOREIGN KEY (ID_quadra)
	REFERENCES Quadras(IDQuadras),

	ID_usuario INT NOT NULL, --Usuário que realizou o pagamento.
	CONSTRAINT FK_Usuario_Pagamentos FOREIGN KEY (ID_usuario)
	REFERENCES Usuario(IDUsuario)
);
GO

CREATE TABLE Agendamentos (
	IDAgendamento INT PRIMARY KEY IDENTITY (1,1),
	DataReserva DATE,
	HoraInicio TIME,
	HoraFim TIME,

	CONSTRAINT VF_Horario_Agendamentos CHECK (HoraFim > HoraInicio), --Garante que o horário de fim sejam maior que o horário de início.

	StatusReserva VARCHAR(40) DEFAULT 'Pendente', --Pendente para viabilidade.
	CONSTRAINT CHK_Status_Reserva
	CHECK (StatusReserva IN ('Em andamento', 'Finalizado', 'Pendente')),

	ID_Usuario INT NOT NULL,
	CONSTRAINT FK_Usuario_Agendamento FOREIGN KEY (ID_Usuario)
	REFERENCES Usuario(IDUsuario),

	ID_Quadra INT NOT NULL,
	CONSTRAINT FK_Quadra_Agendamento FOREIGN KEY (ID_Quadra)
	REFERENCES Quadras(IDQuadras),

	ID_Pagamento INT NOT NULL,
	CONSTRAINT FK_Pagamento_Agendamento FOREIGN KEY (ID_Pagamento)
	REFERENCES Pagamentos(IDPagamento)
);
GO

CREATE TABLE LogExclusaoQuadras (
	ID_Log INT PRIMARY KEY IDENTITY(1,1),
	ID_Quadra INT NOT NULL,
	Nome_Quadra VARCHAR(100) NOT NULL,
	Usuario_Alteracao VARCHAR(100) NOT NULL DEFAULT SUSER_NAME(), -- Login de quem alterou
	Data_Alteracao DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TRIGGER trg_LogExclusaoQuadras
ON Quadras
AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON; --Desativa mensagem de linhas afetadas.

	INSERT INTO LogExclusaoQuadras (ID_Quadra, Nome_Quadra, Usuario_Alteracao, Data_Alteracao)
	SELECT
	d.IDQuadras, --Quadra excluída
	d.NomeQuadra, --Nome da quadra excluída
	SYSTEM_USER, --Usuário do SQL que excluiu
	GETDATE() --Data e hora da exclusão
FROM
	deleted d;
END;
GO

--Trigger para relacionar o ID do usuário à uma quadra e transforma-lo em gestor.
CREATE TRIGGER trg_VirarGestor
ON Quadras
AFTER INSERT
AS
BEGIN
	UPDATE Usuario --Criar a requisição para atualizar a tabela Usuario
	SET Permissao = 'Gestor' --Seta a permissao para gestor na instância
	FROM Usuario --Dentro da tabela Usuario
	INNER JOIN inserted i ON Usuario.IDUsuario = i.ID_gestor --Entra na coluna IDUsuario e vincula ao ID_gestor
	WHERE Usuario.Permissao = 'Cliente'; --Onde estiver com Permissao = 'Cliente'
END;
GO

CREATE TRIGGER trg_ExclusaoSomenteAdmin
ON Usuario
INSTEAD OF DELETE --Ao invés de deletar, faça isso primeiro.
AS
BEGIN
	--Apenas deleta se o usuário conter a permissão "Admin" vinculada ao seu ID.
	IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Permissao = 'Admin' AND IDUsuario = CAST(SESSION_CONTEXT(N'ExecutorID') AS INT))
	BEGIN
		RAISERROR ('Ação negada: Apenas administradores podem realizar essa função', 16, 1);
		ROLLBACK TRANSACTION; --Retorna ao último estado antes da ação.
	END
	ELSE
	BEGIN
		DELETE FROM Usuario WHERE IDUsuario IN (SELECT IDUsuario FROM deleted); --Deleta o usuário caso tenha permissão de Admin.
	END
END;
GO

EXEC sp_set_session_context 'ExecutorID', 1; --Garante que o usuário que está acessando é o Admin para efetuar alterações.
-- Sempre necessário definir na query que for excluir um usuário.
