namespace AluguelQuadrasSUN7;

using System.IO;
public class Endereco
{
    public int Id { get; set; }
    public int QuadraId { get; set; }
    public string Rua { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string pathFileQuadras = "quadras.txt";
    public void CadastrarEndereco()
    {


         string pathFileEnderecos = "enderecos.txt";

            try
            {
                
                if (!File.Exists(pathFileQuadras))
                {
                    Console.WriteLine("Erro: Não é possível cadastrar um endereço porque nenhuma quadra existe.");
                    return;
                }

                string[] linhasQuadras = File.ReadAllLines("quadras.txt");
                bool quadraExiste = false;

                foreach (string linhaQuadra in linhasQuadras)
                {
                    if (string.IsNullOrWhiteSpace(linhaQuadra)) continue;
                    
                    string[] dadosQuadra = linhaQuadra.Split(';');
                int idQuadraDoArquivo = int.Parse(dadosQuadra[0]);

                    if (idQuadraDoArquivo == this.QuadraId)
                    {
    quadraExiste = true;
    break;
}
}

                if (!quadraExiste)
                {
                    Console.WriteLine($"Erro: A Quadra com ID {QuadraId} não existe no sistema. Cadastro abortado.");
                    return;
                }

                
                if (File.Exists(pathFileEnderecos))
{
    string[] linhasEnderecos = File.ReadAllLines(pathFileEnderecos);
    this.Id = linhasEnderecos.Length + 1;
}
else
{
    this.Id = 1;
}


    string dadosEndereco = $"{Id};{QuadraId};{Rua};{Numero};{Bairro};{Cidade};{DateTime.Now}{Environment.NewLine}";

File.AppendAllText(pathFileEnderecos, dadosEndereco);

Console.WriteLine($"\nEndereço cadastrado com sucesso para a Quadra ID {QuadraId}! Endereço ID: {Id}");
            }
            catch (Exception ex)
            {
    Console.WriteLine($"Erro ao cadastrar o endereço: {ex.Message}");
}
    }

    public void AtualizarEndereco()
{
    string arquivoEnderecos = "enderecos.txt";

    if (this.Id <= 0)
    {
        Console.WriteLine("Erro: ID de endereço inválido para atualização.");
        return;
    }

    if (!File.Exists(arquivoEnderecos))
    {
        Console.WriteLine("Erro: Arquivo 'enderecos.txt' não encontrado.");
        return;
    }

    try
    {
        string[] linhas = File.ReadAllLines(arquivoEnderecos);
        bool enderecoEncontrado = false;


        for (int i = 0; i < linhas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(linhas[i])) continue;

            string[] dados = linhas[i].Split(';');
            int idDoArquivo = int.Parse(dados[0]);

            if (idDoArquivo == this.Id)
            {

                linhas[i] = $"{Id};{QuadraId};{Rua};{Numero};{Bairro};{Cidade};{DateTime.Now} (Atualizado)";
                enderecoEncontrado = true;
                break;
            }
        }

        if (enderecoEncontrado)
        {
            File.WriteAllLines(arquivoEnderecos, linhas);
            Console.WriteLine($"\nEndereço ID {Id} atualizado com sucesso no arquivo!");
        }
        else
        {
            Console.WriteLine("Erro: Endereço não encontrado no arquivo para atualização.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao atualizar o endereço: {ex.Message}");
    }
}
    }

