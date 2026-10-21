namespace AluguelQuadrasSUN7;

public enum TipoQuadra
{
    Volei,
    BeachTennis
}

public class Quadra
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoQuadra Tipo { get; set; }
    public bool Disponivel { get; set; }
    private string pathFile = "quadras.txt";

            public void CadastrarQuadra()
    {

            try
            {
                if (File.Exists(pathFile))
                {
                    string[] linhas = File.ReadAllLines(pathFile);
                    this.Id = linhas.Length + 1;
                }
                else
                {
                    this.Id = 1;
                }
                this.Disponivel = true;

                string dadosQuadra = $"{Id}|{Nome}|{Tipo}|{Disponivel}|{DateTime.Now}{Environment.NewLine}";

            
                File.AppendAllText(pathFile, dadosQuadra);

                Console.WriteLine($"\nQuadra '{Nome}' cadastrada com sucesso! ID gerado: {Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao cadastrar a quadra no arquivo: {ex.Message}");
            }
    }

public void AtualizarQuadra()
        {
            string caminhoArquivo = "quadras.txt";

            // Validação básica para garantir que a quadra possua um ID válido antes de atualizar
            if (this.Id <= 0)
            {
                Console.WriteLine("Erro: ID de quadra inválido para atualização.");
                return;
            }

            if (!File.Exists(caminhoArquivo))
            {
                Console.WriteLine("Erro: Arquivo 'quadras.txt' não encontrado.");
                return;
            }

            try
            {
                
                string[] linhas = File.ReadAllLines(caminhoArquivo);
                bool quadraEncontrada = false;

                
                for (int i = 0; i < linhas.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(linhas[i])) continue;

                    string[] dados = linhas[i].Split('|');
                    int idDoArquivo = int.Parse(dados[0]);

                    if (idDoArquivo == this.Id)
                    {
                        
                        linhas[i] = $"{Id}|{Nome}|{Tipo}|{Disponivel}|{DateTime.Now} (Atualizada)";
                        quadraEncontrada = true;
                        break; // Para o laço de repetição
                    }
                }


                
                if (quadraEncontrada)
                {
                    File.WriteAllLines(caminhoArquivo, linhas);
                    Console.WriteLine($"\nQuadra ID {Id} atualizada com sucesso no arquivo!");
                }
                else
                {
                    Console.WriteLine("Erro: Quadra não encontrada no arquivo para atualização.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar a quadra: {ex.Message}");
            }
        }

        
    }
    
    
