CREATE PROCEDURE CadastrarAgendamento
    @DataReserva DATE,
    @HoraInicio TIME,
    @HoraFim TIME,
    @ID_Usuario INT,
    @ID_Quadra INT,
    @ID_Pagamento INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validação de hora fim maior que inicio.
        IF @HoraFim <= @HoraInicio
        BEGIN
            RAISERROR('Erro: O horário de término deve ser maior que o horário de início.', 16, 1);
            RETURN;
        END

        -- Verifica se a quadra está disponível naquele horário.
        IF EXISTS (
            SELECT 1 
            FROM Agendamentos 
            WHERE ID_Quadra = @ID_Quadra 
              AND DataReserva = @DataReserva
              AND (
                   (@HoraInicio >= HoraInicio AND @HoraInicio < HoraFim) OR -- Novo inicio está dentro de um jogo existente
                   (@HoraFim > HoraInicio AND @HoraFim <= HoraFim) OR       -- Novo fim verifica um jogo existente
                   (HoraInicio >= @HoraInicio AND HoraFim <= @HoraFim)      -- O novo horário "abraça" um jogo existente por fora
              )
        )
        BEGIN
            RAISERROR('Erro: Esta quadra já possui um agendamento para o horário selecionado.', 16, 2);
            RETURN;
        END

        -- Verificou os horários, tenta inserir.
        INSERT INTO Agendamentos (DataReserva, HoraInicio, HoraFim, ID_Usuario, ID_Quadra, ID_Pagamento)
        VALUES (@DataReserva, @HoraInicio, @HoraFim, @ID_Usuario, @ID_Quadra, @ID_Pagamento);

        PRINT 'Agendamento realizado com sucesso!';
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorNumber INT = ERROR_NUMBER();

        IF @ErrorNumber = 547
        BEGIN
            RAISERROR('Erro de Vínculo: O Usuário, Quadra ou Pagamento informado não existe.', 16, 3);
        END
        ELSE
        BEGIN
            -- Caso seja o erro do RAISERROR ele repassa a mensagem correta.
            THROW; 
        END
    END CATCH
END;
GO
