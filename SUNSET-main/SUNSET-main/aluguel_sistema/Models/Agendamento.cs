using System;

namespace AluguelQuadrasSUN7;

public enum StatusAgendamento
{
    Livre,
    Reservado,
    Concluido
}

public class Agendamento
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int QuadraId { get; set; }
    public int PagamentoId { get; set; }
    public TimeSpan HorarioInicio { get; set; }
    public TimeSpan HorarioFim { get; set; }
    public DateTime Data { get; set; }
    public StatusAgendamento Status { get; set; }

    public void FazerAgendamento()
    {
        string arquivoUsuarios = "usuarios.txt";
            string arquivoQuadras = "quadras.txt";
            string arquivoAgendamentos = "agendamentos.txt";

            try
            {
                if (!File.Exists(arquivoUsuarios))
                {
                    Console.WriteLine("Erro: Nenhum usuário cadastrado no sistema.");
                    return;
                }
                string[] linhasUsuarios = File.ReadAllLines(arquivoUsuarios);
                bool usuarioExiste = false;
                foreach (var linha in linhasUsuarios)
                {
                    if (string.IsNullOrWhiteSpace(linha)) continue;
                    if (int.Parse(linha.Split(';')[0]) == this.UsuarioId) { usuarioExiste = true; break; }
                }

                if (!usuarioExiste)
                {
                    Console.WriteLine($"Erro: O Usuário com ID {UsuarioId} não foi encontrado.");
                    return;
                }

                if (!File.Exists(arquivoQuadras))
                {
                    Console.WriteLine("Erro: Nenhuma quadra cadastrada no sistema.");
                    return;
                }
                string[] linhasQuadras = File.ReadAllLines(arquivoQuadras);
                bool quadraExiste = false;
                foreach (var linha in linhasQuadras)
                {
                    if (string.IsNullOrWhiteSpace(linha)) continue;
                    if (int.Parse(linha.Split(';')[0]) == this.QuadraId) { quadraExiste = true; break; }
                }

                if (!quadraExiste)
                {
                    Console.WriteLine($"Erro: A Quadra com ID {QuadraId} não foi encontrada.");
                    return;
                }

                if (File.Exists(arquivoAgendamentos))
                {
                    string[] linhasAgendamentos = File.ReadAllLines(arquivoAgendamentos);
                    this.Id = linhasAgendamentos.Length + 1;
                }
                else
                {
                    this.Id = 1;
                }

                this.Status = StatusAgendamento.Reservado;

                string dataFormatada = this.Data.ToString("dd/MM/yyyy");

                string dadosAgendamento = $"{Id};{UsuarioId};{QuadraId};{PagamentoId};{HorarioInicio};{HorarioFim};{dataFormatada};{Status};{DateTime.Now}{Environment.NewLine}";

                File.AppendAllText(arquivoAgendamentos, dadosAgendamento);

                Console.WriteLine($"\nAgendamento realizado com SUCESSO!");
                Console.WriteLine($"Reserva ID: {Id} | Quadra ID: {QuadraId} | Horário: {HorarioInicio} às {HorarioFim} na data {dataFormatada}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar agendamento: {ex.Message}");
            }
    }

    public void CancelarAgendamento()
    {
        string arquivoAgendamentos = "agendamentos.txt";

            if (this.Id <= 0)
            {
                Console.WriteLine("Erro: ID de agendamento inválido para cancelamento.");
                return;
            }

            if (!File.Exists(arquivoAgendamentos))
            {
                Console.WriteLine("Erro: Nenhum agendamento encontrado no sistema.");
                return;
            }

            try
            {
                string[] linhas = File.ReadAllLines(arquivoAgendamentos);
                bool agendamentoEncontrado = false;

                for (int i = 0; i < linhas.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(linhas[i])) continue;

                    string[] dados = linhas[i].Split(';');
                    int idDoArquivo = int.Parse(dados[0]);

                    if (idDoArquivo == this.Id)
                    {
                        
                        this.Status = StatusAgendamento.Livre;

                        string dataFormatada = this.Data.ToString("dd/MM/yyyy");

            
                        linhas[i] = $"{Id};{UsuarioId};{QuadraId};{PagamentoId};{HorarioInicio};{HorarioFim};{dataFormatada};{Status};{DateTime.Now} (Cancelado)";
                        agendamentoEncontrado = true;
                        break;
                    }
                }

                if (agendamentoEncontrado)
                {
                    File.WriteAllLines(arquivoAgendamentos, linhas);
                    Console.WriteLine($"\nAgendamento ID {Id} cancelado com sucesso! O horário está livre novamente.");
                }
                else
                {
                    Console.WriteLine("Erro: Agendamento não encontrado para cancelamento.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao cancelar o agendamento: {ex.Message}");
            }
    }
}
