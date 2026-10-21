namespace AluguelQuadrasSUN7;

public enum TipoPagamento
{
    Pix
}

public class Pagamento
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public string Status { get; set; } = string.Empty;
    public TipoPagamento TipoPagamento { get; set; }
    public int TempoLimiteMinutos { get; set; } = 5;

    public void RealizarPagamento()
    {
        string arquivoPagamentos = "pagamentos.txt";

            try
            {
                
                if (File.Exists(arquivoPagamentos))
                {
                    string[] linhas = File.ReadAllLines(arquivoPagamentos);
                    this.Id = linhas.Length + 1;
                }
                else
                {
                    this.Id = 1;
                }

                this.Status = "Pendente";
                this.TipoPagamento = TipoPagamento.Pix;

                GerarQRCodePIX();

                string dadosPagamento = $"{Id};{Valor};{Status};{TipoPagamento};{TempoLimiteMinutos};{DateTime.Now}{Environment.NewLine}";
                File.AppendAllText(arquivoPagamentos, dadosPagamento);

                Console.WriteLine($"\nPagamento ID {Id} registrado como PENDENTE.");
                Console.WriteLine($"Você tem {TempoLimiteMinutos} minutos para pagar.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o pagamento: {ex.Message}");
            }
    }

    public void ConfirmarPagamento()
    {
        string arquivoPagamentos = "pagamentos.txt";

            if (this.Id <= 0)
            {
                Console.WriteLine("Erro: ID de pagamento inválido.");
                return;
            }

            if (!File.Exists(arquivoPagamentos))
            {
                Console.WriteLine("Erro: Nenhum registro de pagamento encontrado.");
                return;
            }

            try
            {
                // 1. Ler todos os pagamentos para a memória
                string[] linhas = File.ReadAllLines(arquivoPagamentos);
                bool pagamentoEncontrado = false;

                // 2. Localizar o ID deste pagamento
                for (int i = 0; i < linhas.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(linhas[i])) continue;

                    string[] dados = linhas[i].Split(';');
                    int idDoArquivo = int.Parse(dados[0]);

                    if (idDoArquivo == this.Id)
                    {
                        // Atualiza o status na instância atual
                        this.Status = "Aprovado";

                        // 3. Modifica a linha no arquivo de texto mantendo os outros dados intactos
                        linhas[i] = $"{Id};{Valor};{Status};{TipoPagamento};{TempoLimiteMinutos};{DateTime.Now} (Confirmado)";
                        pagamentoEncontrado = true;
                        break;
                    }
                }

                // 4. Grava as alterações de volta no arquivo plano
                if (pagamentoEncontrado)
                {
                    File.WriteAllLines(arquivoPagamentos, linhas);
                    Console.WriteLine($"\nSucesso: O Pagamento ID {Id} foi CONFIRMADO e APROVADO!");
                }
                else
                {
                    Console.WriteLine("Erro: Pagamento não localizado para confirmação.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao confirmar o pagamento: {ex.Message}");
            }
    }

    public void GerarQRCodePIX()
    {
        //
    }
}
