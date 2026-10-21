using AluguelQuadrasSUN7;

  class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Bem-vindo ao sistema de aluguel de quadras SUN7!");

        while (true)
        {
            Console.WriteLine("\nSelecione uma opção:");
            Console.WriteLine("1. Cadastrar novo usuário");
            Console.WriteLine("2. Fazer login");
            Console.WriteLine("3. Sair");

            string opcao = Console.ReadLine() ?? string.Empty;

            switch (opcao)
            {
                case "1":
                    System.Console.Write("Digite seu nome: ");
                     string nome = Console.ReadLine() ?? string.Empty;
                    System.Console.Write("Digite seu sobrenome: ");
                    string sobrenome = Console.ReadLine() ?? string.Empty;
                    System.Console.Write("Digite seu cpf: ");
                    string cpf = Console.ReadLine() ?? string.Empty;
                    System.Console.Write("Digite seu email: ");
                    string email = Console.ReadLine() ?? string.Empty;
                    System.Console.Write("Digite sua senha: ");
                    string senha = Console.ReadLine() ?? string.Empty;

                    Usuario NovoUsuario = new Usuario
                    {
                      Nome = nome,
                      Sobrenome = sobrenome,
                      Cpf = cpf,
                      Email = email,
                      Senha = senha,
                    };
                    
                    NovoUsuario.Cadastrar();
                    break;
                case "2":
                    System.Console.WriteLine("Qual o nome da quadra: ");
                    string nomeq = Console.ReadLine() ?? string.Empty; 
                    System.Console.WriteLine("Qual o nome da quadra: ");
                    string tipoq = Console.ReadLine() ?? string.Empty;

                    Quadra QuadraNova = new Quadra
                    {
                        Nome = nomeq,
                        Tipo = TipoQuadra.Parse<TipoQuadra>(tipoq)
                    };
                    QuadraNova.CadastrarQuadra();

                    break;
                case "3":
                    Console.WriteLine("Obrigado por usar o sistema! Até mais!");
                    return;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        }
    }
    
}