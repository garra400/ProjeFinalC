using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

// Definição das entidades do banco de dados...

public class ProjetoDbContext : DbContext
{
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Projeto> Projetos { get; set; }
    public DbSet<ProjetoUsuario> ProjetosUsuarios { get; set; }
    public DbSet<Tarefa> Tarefas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySQL("server=localhost;user=root;password=senha");

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração das chaves compostas...

        // Configuração da tabela associativa ProjetoUsuario
        modelBuilder.Entity<ProjetoUsuario>()
            .HasKey(pu => new { pu.UsuarioId, pu.ProjetoId });

        modelBuilder.Entity<ProjetoUsuario>()
            .HasOne(pu => pu.Usuario)
            .WithMany(u => u.ProjetosUsuarios)
            .HasForeignKey(pu => pu.UsuarioId);

        modelBuilder.Entity<ProjetoUsuario>()
            .HasOne(pu => pu.Projeto)
            .WithMany(p => p.ProjetosUsuarios)
            .HasForeignKey(pu => pu.ProjetoId);

        base.OnModelCreating(modelBuilder);
    }

    public void CriarBancoDeDados()
    {
        if (!Database.CanConnect())
        {
            Console.WriteLine("Banco de dados não encontrado. Criando banco de dados...");

            Database.EnsureCreated();

            var categorias = new List<Categoria>
            {
                new Categoria { Nome = "Não iniciada" },
                new Categoria { Nome = "Em andamento" },
                new Categoria { Nome = "Finalizada" }
            };

            Categorias.AddRange(categorias);
            SaveChanges();

            Console.WriteLine("Banco de dados criado com sucesso.");
        }
    }
}

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Sistema de Gerenciamento de Projetos");

        using (var db = new ProjetoDbContext())
        {
            db.CriarBancoDeDados();

            // Login ou registro do usuário
            Usuario usuario = LoginRegistro(db);

            // Menu principal
            bool sair = false;
            while (!sair)
            {
                Console.WriteLine();
                Console.WriteLine("1. Criar Projeto");
                Console.WriteLine("2. Listar Projetos");
                Console.WriteLine("3. Adicionar Amigo");
                Console.WriteLine("4. Listar Amigos");
                Console.WriteLine("5. Sair");
                Console.WriteLine();

                Console.Write("Digite a opção desejada: ");
                string opcao = Console.ReadLine();

                switch (opcao)
                {
                    case "1":
                        CriarProjeto(usuario, db);
                        break;
                    case "2":
                        ListarProjetos(usuario, db);
                        break;
                    case "3":
                        AdicionarAmigo(usuario, db);
                        break;
                    case "4":
                        ListarAmigos(usuario, db);
                        break;
                    case "5":
                        sair = true;
                        break;
                    default:
                        Console.WriteLine("Opção inválida. Tente novamente.");
                        break;
                }
            }
        }

        Console.WriteLine("Programa encerrado.");
    }

    static Usuario LoginRegistro(ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.WriteLine("1. Fazer Login");
        Console.WriteLine("2. Registrar-se");
        Console.WriteLine();

        Console.Write("Digite a opção desejada: ");
        string opcao = Console.ReadLine();

        Usuario usuario = null;

        switch (opcao)
        {
            case "1":
                usuario = FazerLogin(db);
                break;
            case "2":
                usuario = RegistrarUsuario(db);
                break;
            default:
                Console.WriteLine("Opção inválida. Tente novamente.");
                usuario = LoginRegistro(db);
                break;
        }

        return usuario;
    }

    static Usuario FazerLogin(ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.Write("Digite seu email: ");
        string email = Console.ReadLine();

        Console.Write("Digite sua senha: ");
        string senha = Console.ReadLine();

        Usuario usuario = db.Usuarios.FirstOrDefault(u => u.Email == email && u.Senha == senha);

        if (usuario == null)
        {
            Console.WriteLine("Email ou senha incorretos. Tente novamente.");
            return FazerLogin(db);
        }

        Console.WriteLine($"Bem-vindo, {usuario.Nome}!");

        return usuario;
    }

    static Usuario RegistrarUsuario(ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.Write("Digite seu nome: ");
        string nome = Console.ReadLine();

        Console.Write("Digite seu email: ");
        string email = Console.ReadLine();

        Console.Write("Digite uma senha: ");
        string senha = Console.ReadLine();

        Usuario usuario = new Usuario
        {
            Nome = nome,
            Email = email,
            Senha = senha
        };

        db.Usuarios.Add(usuario);
        db.SaveChanges();

        Console.WriteLine("Registro concluído com sucesso!");

        return usuario;
    }

    static void CriarProjeto(Usuario usuario, ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.Write("Digite o nome do projeto: ");
        string nome = Console.ReadLine();

        Projeto projeto = new Projeto
        {
            Nome = nome,
            UsuarioResponsavelId = usuario.UsuarioId
        };

        db.Projetos.Add(projeto);
        db.SaveChanges();

        Console.WriteLine("Projeto criado com sucesso!");
    }

    static void ListarProjetos(Usuario usuario, ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.WriteLine("Projetos do usuário:");

        var projetos = db.Projetos.Where(p => p.UsuarioResponsavelId == usuario.UsuarioId).ToList();

        foreach (var projeto in projetos)
        {
            Console.WriteLine($"- {projeto.Nome}");
        }
    }

    static void AdicionarAmigo(Usuario usuario, ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.Write("Digite o nome do amigo: ");
        string nomeAmigo = Console.ReadLine();

        var amigo = db.Usuarios.FirstOrDefault(u => u.Nome == nomeAmigo);

        if (amigo == null)
        {
            Console.WriteLine("Usuário não encontrado.");
            return;
        }

        var amizadeExistente = db.Amizades.FirstOrDefault(a => (a.UsuarioId == usuario.UsuarioId && a.AmigoId == amigo.UsuarioId) ||
                                                                (a.UsuarioId == amigo.UsuarioId && a.AmigoId == usuario.UsuarioId));

        if (amizadeExistente != null)
        {
            Console.WriteLine("Você já é amigo dessa pessoa.");
            return;
        }

        Amizade amizade = new Amizade
        {
            UsuarioId = usuario.UsuarioId,
            AmigoId = amigo.UsuarioId
        };

        db.Amizades.Add(amizade);
        db.SaveChanges();

        Console.WriteLine($"Você adicionou {amigo.Nome} como amigo.");
    }

    static void ListarAmigos(Usuario usuario, ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.WriteLine("Seus amigos:");

        var amigos = (from a in db.Amizades
                      join u in db.Usuarios on a.AmigoId equals u.UsuarioId
                      where a.UsuarioId == usuario.UsuarioId
                      select u).ToList();

        foreach (var amigo in amigos)
        {
            Console.WriteLine($"- {amigo.Nome}");
        }
    }
}
