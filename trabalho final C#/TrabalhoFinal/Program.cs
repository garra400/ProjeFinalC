using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data.SqlClient;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
    public List<ProjetoUsuario> ProjetosUsuarios { get; set; }
    public List<TarefaUsuario> TarefasUsuarios { get; set; }
    public Usuario()
    {
        ProjetosUsuarios = new List<ProjetoUsuario>(); // Inicialize a lista
        TarefasUsuarios = new List<TarefaUsuario>(); // Inicialize a lista
    }
}

public class Projeto
{
    public int ProjetoId { get; set; }
    public string Nome { get; set; }
    public int UsuarioResponsavelId { get; set; }
    public Usuario UsuarioResponsavel { get; set; }
    public List<ProjetoUsuario> ProjetosUsuarios { get; set; }
    public List<Tarefa> Tarefas { get; set; }
    public string Status { get; set; } // Adicionada a propriedade "Status"
    public Projeto()
    {
        ProjetosUsuarios = new List<ProjetoUsuario>(); // Inicialize a lista
        Tarefas = new List<Tarefa>(); // Inicialize a lista
    }
}

public class Tarefa
{
    public int TarefaId { get; set; }
    public string Nome { get; set; }
    public int ProjetoId { get; set; }
    public Projeto Projeto { get; set; }
    public List<TarefaUsuario> TarefasUsuarios { get; set; }
    public Tarefa()
    {
        TarefasUsuarios = new List<TarefaUsuario>(); // Inicialize a lista
    }
}

public class ProjetoUsuario
{
    public int ProjetoId { get; set; }
    public Projeto Projeto { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }
    public bool IsGerente { get; set; }
}

public class TarefaUsuario
{
    public int TarefaId { get; set; }
    public Tarefa Tarefa { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }
    public bool IsGerente { get; set; }
}

public class ProjetoDbContext : DbContext
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Projeto> Projetos { get; set; }
    public DbSet<Tarefa> Tarefas { get; set; }
    public DbSet<ProjetoUsuario> ProjetosUsuarios { get; set; }
    public DbSet<TarefaUsuario> TarefasUsuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql("server=localhost;user=root;database=db_GerenciadordeProjetos", new MySqlServerVersion(new Version(8, 0, 26)))
            .EnableSensitiveDataLogging(); // Add this line to enable sensitive data logging

        base.OnConfiguring(optionsBuilder);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjetoUsuario>()
            .HasKey(pu => new { pu.ProjetoId, pu.UsuarioId });

        modelBuilder.Entity<ProjetoUsuario>()
            .HasOne(pu => pu.Projeto)
            .WithMany(p => p.ProjetosUsuarios)
            .HasForeignKey(pu => pu.ProjetoId);

        modelBuilder.Entity<ProjetoUsuario>()
            .HasOne(pu => pu.Usuario)
            .WithMany(u => u.ProjetosUsuarios)
            .HasForeignKey(pu => pu.UsuarioId);

        modelBuilder.Entity<TarefaUsuario>()
            .HasKey(tu => new { tu.TarefaId, tu.UsuarioId });

        modelBuilder.Entity<TarefaUsuario>()
            .HasOne(tu => tu.Tarefa)
            .WithMany(t => t.TarefasUsuarios)
            .HasForeignKey(tu => tu.TarefaId);

        modelBuilder.Entity<TarefaUsuario>()
            .HasOne(tu => tu.Usuario)
            .WithMany(u => u.TarefasUsuarios)
            .HasForeignKey(tu => tu.UsuarioId);
    }
}

public class Program
{
    static void Main(string[] args)
    {
        using (var db = new ProjetoDbContext())
        {
            bool sair = false;
            Usuario usuarioLogado = null;
            Projeto projetoSelecionado = null;

            while (!sair)
            {
                Console.Clear();
                Console.WriteLine("== Sistema de Gerenciamento de Projetos ==");
                Console.WriteLine();

                if (usuarioLogado == null)
                {
                    Console.WriteLine("1. Registrar");
                    Console.WriteLine("2. Login");
                    Console.WriteLine("0. Sair");
                }
                else
                {
                    Console.WriteLine($"Bem-vindo, {usuarioLogado.Nome}!");
                    Console.WriteLine("1. Criar Projeto");
                    Console.WriteLine("2. Listar Projetos");
                    Console.WriteLine("3. Selecionar Projeto");
                    Console.WriteLine("0. Logout");
                }

                Console.WriteLine();
                Console.Write("Opção: ");
                string opcaoPrincipal = Console.ReadLine();

                Console.Clear();

                switch (opcaoPrincipal)
                {
                    case "1":
                        if (usuarioLogado == null)
                        {
                            usuarioLogado = Registrar(db);
                        }
                        else
                        {
                            CriarProjeto(usuarioLogado, db);
                        }
                        break;
                    case "2":
                        ListarProjetos(usuarioLogado, db);
                        break;
                    case "3":
                        if (usuarioLogado != null)
                        {
                            projetoSelecionado = SelecionarProjeto(usuarioLogado, db);
                            if (projetoSelecionado != null)
                            {
                                AcessarProjeto(usuarioLogado, projetoSelecionado, db);
                            }
                        }
                        break;
                    case "0":
                        if (usuarioLogado == null)
                        {
                            sair = true;
                        }
                        else
                        {
                            usuarioLogado = null;
                            projetoSelecionado = null;
                        }
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("Pressione qualquer tecla para continuar...");
                Console.ReadKey();
            }
        }
    }

    static Usuario Registrar(ProjetoDbContext db)
    {
        Console.WriteLine("== Registrar Usuário ==");

        Console.Write("Digite seu nome: ");
        string nome = Console.ReadLine();

        Console.Write("Digite seu email: ");
        string email = Console.ReadLine();

        Console.Write("Digite sua senha: ");
        string senha = Console.ReadLine();

        Usuario usuario = new Usuario
        {
            Nome = nome,
            Email = email,
            Senha = senha
        };

        db.Usuarios.Add(usuario);
        db.SaveChanges();

        Console.WriteLine("Usuário registrado com sucesso!");

        return usuario;
    }

    static void CriarProjeto(Usuario usuario, ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.Write("Digite o nome do projeto: ");
        string nomeProjeto = Console.ReadLine();

        Projeto projeto = new Projeto
        {
            Nome = nomeProjeto,
            UsuarioResponsavelId = usuario.UsuarioId
        };

        db.Projetos.Add(projeto);
        db.SaveChanges();

        Console.WriteLine($"Projeto '{nomeProjeto}' criado com sucesso!");
    }

    static void ListarProjetos(Usuario usuario, ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.WriteLine("Projetos:");

        var projetos = new List<Projeto>();

        if (usuario != null)
        {
            projetos = db.Projetos
                .Where(p => p.UsuarioResponsavelId == usuario.UsuarioId)
                .ToList();
        }


        if (projetos.Count == 0)
        {
            Console.WriteLine("Nenhum projeto encontrado.");
        }
        else
        {
            foreach (var projeto in projetos)
            {
                Console.WriteLine($"ID: {projeto.ProjetoId}, Nome: {projeto.Nome}");
            }
        }
    }

    static Projeto SelecionarProjeto(Usuario usuario, ProjetoDbContext db)
    {
        Console.WriteLine();
        Console.Write("Digite o ID do projeto: ");
        string idProjeto = Console.ReadLine();

        if (!int.TryParse(idProjeto, out int projetoId))
        {
            Console.WriteLine("ID inválido.");
            return null;
        }

        var projeto = db.Projetos.FirstOrDefault(p => p.ProjetoId == projetoId && p.UsuarioResponsavelId == usuario.UsuarioId);

        if (projeto == null)
        {
            Console.WriteLine("Projeto não encontrado.");
            return null;
        }

        return projeto;
    }

    static void AcessarProjeto(Usuario usuario, Projeto projeto, ProjetoDbContext db)
    {
        bool sair = false;

        while (!sair)
        {
            Console.WriteLine($"== Projeto: {projeto.Nome} ==");
            Console.WriteLine();

            Console.WriteLine("1. Configurar Projeto");
            Console.WriteLine("2. Adicionar Pessoa ao Projeto");
            Console.WriteLine("3. Adicionar Tarefa ao Projeto");
            Console.WriteLine("4. Listar Tarefas do Projeto");
            Console.WriteLine("0. Voltar");

            Console.WriteLine();
            Console.Write("Opção: ");
            string opcaoProjeto = Console.ReadLine();

            Console.Clear();

            switch (opcaoProjeto)
            {
                case "1":
                    if (usuario.UsuarioId == projeto.UsuarioResponsavelId || IsGerenteOuSuperior(usuario, projeto, db))
                    {
                        ConfigurarProjeto(projeto, db);
                    }
                    else
                    {
                        Console.WriteLine("Você não tem permissão para configurar o projeto.");
                    }
                    break;
                case "2":
                    if (usuario.UsuarioId == projeto.UsuarioResponsavelId || IsGerenteOuSuperior(usuario, projeto, db))
                    {
                        AdicionarPessoaAoProjeto(projeto, db);
                    }
                    else
                    {
                        Console.WriteLine("Você não tem permissão para adicionar pessoas ao projeto.");
                    }
                    break;
                case "3":
                    if (usuario.UsuarioId == projeto.UsuarioResponsavelId || IsGerenteOuSuperior(usuario, projeto, db))
                    {
                        AdicionarTarefaAoProjeto(projeto, db);
                    }
                    else
                    {
                        Console.WriteLine("Você não tem permissão para adicionar tarefas ao projeto.");
                    }
                    break;
                case "4":
                    ListarTarefasProjeto(usuario, projeto, db);
                    break;
                case "0":
                    sair = true;
                    break;
                default:
                    Console.WriteLine("Opção inválida.");
                    continue;
            }

            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para continuar...");
            Console.ReadKey();
        }
    }

    static void ConfigurarProjeto(Projeto projeto, ProjetoDbContext db)
    {
        Console.WriteLine("== Configurar Projeto ==");
        Console.WriteLine();
        Console.WriteLine($"ID: {projeto.ProjetoId}");
        Console.WriteLine($"Nome: {projeto.Nome}");
        Console.WriteLine();
        Console.WriteLine("1. Alterar status do projeto");
        Console.WriteLine("0. Voltar");
        Console.WriteLine();
        Console.Write("Opção: ");
        string opcaoConfig = Console.ReadLine();
        Console.Clear();

        switch (opcaoConfig)
        {
            case "1":
                Console.WriteLine($"Status atual: {projeto.Status}");
                Console.Write("Digite o novo status (Concluído, Cancelado): ");
                string novoStatus = Console.ReadLine();

                if (novoStatus == "Concluído" || novoStatus == "Cancelado")
                {
                    projeto.Status = novoStatus;
                    db.SaveChanges();
                    Console.WriteLine("Status do projeto alterado com sucesso!");
                }
                else
                {
                    Console.WriteLine("Status inválido.");
                }
                break;
            case "0":
                break;
            default:
                Console.WriteLine("Opção inválida.");
                break;
        }
    }

static void AdicionarPessoaAoProjeto(Projeto projeto, ProjetoDbContext db)
    {
        Console.WriteLine("== Adicionar Pessoa ao Projeto ==");
        Console.Write("Digite o email da pessoa a ser adicionada: ");
        string emailPessoa = Console.ReadLine();
        
        var pessoa = db.Usuarios.FirstOrDefault(u => u.Email == emailPessoa);

        if (pessoa == null)
        {
            Console.WriteLine("Usuário não encontrado.");
            return;
        }
        

        var projetoUsuario = new ProjetoUsuario
        {
            Projeto = projeto,
            Usuario = pessoa
        };

        projeto.ProjetosUsuarios.Add(projetoUsuario);
        db.SaveChanges();

        Console.WriteLine($"Pessoa '{pessoa.Nome}' adicionada ao projeto '{projeto.Nome}' com sucesso!");
    }

    static void AdicionarTarefaAoProjeto(Projeto projeto, ProjetoDbContext db)
    {
        Console.WriteLine("== Adicionar Tarefa ao Projeto ==");

        Console.Write("Digite o nome da tarefa: ");
        string nomeTarefa = Console.ReadLine();

        var tarefa = new Tarefa
        {
            Nome = nomeTarefa,
            ProjetoId = projeto.ProjetoId
        };

        db.Tarefas.Add(tarefa);
        db.SaveChanges();

        Console.WriteLine("Tarefa adicionada ao projeto com sucesso!");
    }

    static void ListarTarefasProjeto(Usuario? usuario, Projeto? projeto, ProjetoDbContext db)
    {
        if (usuario == null || projeto == null)
        {
            Console.WriteLine("Nenhum usuário ou projeto selecionado.");
            return;
        }
        Console.WriteLine();
        Console.WriteLine("Tarefas do Projeto:");

        var tarefas = db.Tarefas.Where(t => t.ProjetoId == projeto.ProjetoId).ToList();

        if (tarefas.Count == 0)
        {
            Console.WriteLine("Nenhuma tarefa encontrada.");
        }
        else
        {
            foreach (var tarefa in tarefas)
            {
                Console.WriteLine($"ID: {tarefa.TarefaId}, Nome: {tarefa.Nome}");
            }
        }
    }

    static bool IsGerenteOuSuperior(Usuario usuario, Projeto projeto, ProjetoDbContext db)
    {
        var projetoUsuario = db.ProjetosUsuarios.FirstOrDefault(pu => pu.ProjetoId == projeto.ProjetoId && pu.UsuarioId == usuario.UsuarioId);

        return projetoUsuario != null && projetoUsuario.IsGerente;
    }
}
