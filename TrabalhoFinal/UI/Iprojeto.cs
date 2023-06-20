using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data.SqlClient;
using TrabalhoFinal.Models;
using TrabalhoFinal.Context;

namespace TrabalhoFinal.UI
{
    public class ProjetoUI
    {
        public static void CriarProjeto(Usuario Usuario, ProjetoDbContext db)
        {
            Console.WriteLine();
            Console.Write("Digite o nome do Projeto: ");
            string? nomeProjeto = Console.ReadLine();
            if (nomeProjeto == null || nomeProjeto ==""){
                Console.WriteLine("Digite um nome válido para o Projeto!");
                return;
            }
            Console.Write("Digite a descrição: ");
            string? descricaoProjeto = Console.ReadLine();
            if (descricaoProjeto == null || descricaoProjeto == ""){
                Console.WriteLine("Digite uma Descroção válida para a Tarefa!");
                return;
            }

            Projeto Projeto = new Projeto
            {
                Nome = nomeProjeto,
                UsuarioResponsavelId = Usuario.UsuarioId,
                Status = "Em andamento",
                Descricao = descricaoProjeto
            };

            db.Projetos?.Add(Projeto);
            db.SaveChanges();

            Console.WriteLine($"Projeto '{nomeProjeto}' criado com sucesso!");
        }

        public static void ListarProjetos(Usuario Usuario, ProjetoDbContext db)
        {
            Console.WriteLine();
            Console.WriteLine("Projetos:");

            var Projetos = new List<Projeto>();
            var projetoUsuarios = new List<ProjetoUsuario>();

            if (Usuario != null)
            {
                Projetos = db.Projetos?
                .Where(p => p.UsuarioResponsavelId == Usuario.UsuarioId ||
                        p.ProjetosUsuarios.Any(pu => pu.UsuarioId == Usuario.UsuarioId))
                .ToList();
            }


            if (Projetos?.Count == 0)
            {
                Console.WriteLine("Nenhum Projeto encontrado.");
            }
            else
            {
                for (int i = 0; i < Projetos?.Count; i++)
                {
                    Projeto? Projeto = Projetos[i];
                    Console.WriteLine($"ID: {Projeto.ProjetoId}, Nome: {Projeto.Nome}, Status: {Projeto.Status}, Descrição: {Projeto.Descricao} ");
                }
            }
        }

        public static Projeto? SelecionarProjeto(Usuario Usuario, ProjetoDbContext db)
        {
            Console.WriteLine();
            Console.Write("Digite o ID do Projeto: ");
            string? idProjeto = Console.ReadLine();
            if (idProjeto == null){
                Console.WriteLine("ID inválido.");
                return null;
            }

            if (!int.TryParse(idProjeto, out int ProjetoId))
            {
                Console.WriteLine("ID inválido.");
                return null;
            }

            var Projeto = db.Projetos?
            .Include(p => p.ProjetosUsuarios)
            .FirstOrDefault(p => p.ProjetoId == ProjetoId &&
                             (p.UsuarioResponsavelId == Usuario.UsuarioId ||
                              p.ProjetosUsuarios.Any(pu => pu.UsuarioId == Usuario.UsuarioId)));

            if (Projeto == null )
            {
                Console.WriteLine("Projeto não encontrado.");
                return null;
            }

            return Projeto;
        }

        public static void AcessarProjeto(Usuario Usuario, Projeto Projeto, ProjetoDbContext db)
        {
            bool sair = false;

            while (!sair)
            {
                Console.WriteLine($"== Projeto: {Projeto.Nome} // Status: {Projeto.Status} // Descrição: {Projeto.Descricao} ==");
                Console.WriteLine();
                Console.WriteLine("1. Configurar Projeto");
                Console.WriteLine("2. Adicionar Pessoa ao Projeto");
                Console.WriteLine("3. Adicionar Tarefa ao Projeto");
                Console.WriteLine("4. Listar Tarefas do Projeto"); // Adicionada a opção para listar tarefas
                Console.WriteLine("0. Voltar");
                Console.WriteLine();
                Console.Write("Opção: ");
                string? opcaoProjeto = Console.ReadLine();

                Console.Clear();

                switch (opcaoProjeto)
                {
                    case "1":
                        if (Usuario.UsuarioId == Projeto.UsuarioResponsavelId || IsGerenteOuSuperior(Usuario, Projeto, db))
                        {
                            ConfigurarProjeto(Projeto, db);
                        }
                        else
                        {
                            Console.WriteLine("Você não tem permissão para configurar o Projeto.");
                        }
                        break;
                    case "2":
                        if (Usuario.UsuarioId == Projeto.UsuarioResponsavelId || IsGerenteOuSuperior(Usuario, Projeto, db))
                        {
                            AdicionarPessoaAoProjeto(Projeto, db);
                        }
                        else
                        {
                            Console.WriteLine("Você não tem permissão para adicionar pessoas ao Projeto.");
                        }
                        break;
                    case "3":
                        if (Usuario.UsuarioId == Projeto.UsuarioResponsavelId || IsGerenteOuSuperior(Usuario, Projeto, db))
                        {
                            AdicionarTarefaAoProjeto(Projeto, db);
                        }
                        else
                        {
                            Console.WriteLine("Você não tem permissão para adicionar tarefas ao Projeto.");
                        }
                        break;
                    case "4":
                        ListarTarefasProjeto(Usuario, Projeto, db);
                        break; // Adicionada a opção para listar tarefas
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

        public static void ConfigurarProjeto(Projeto Projeto, ProjetoDbContext db)
        {
            Console.WriteLine("== Configurar Projeto ==");
            Console.WriteLine();
            Console.WriteLine($"ID: {Projeto.ProjetoId}");
            Console.WriteLine($"Nome: {Projeto.Nome}");
            Console.WriteLine();
            Console.WriteLine("1. Alterar status do Projeto");
            Console.WriteLine("0. Voltar");
            Console.WriteLine();
            Console.Write("Opção: ");
            string? opcaoConfig = Console.ReadLine();
            Console.Clear();

            switch (opcaoConfig)
            {
                case "1":
                    Console.WriteLine($"Status atual: {Projeto.Status}");
                    Console.Write("Digite o novo status (Concluído, Cancelado): ");
                    string? novoStatus = Console.ReadLine();

                    if (novoStatus == "Concluído" || novoStatus == "Cancelado")
                    {
                        Projeto.Status = novoStatus;
                        db.SaveChanges();
                        Console.WriteLine("Status do Projeto alterado com sucesso!");
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


        public static void AdicionarPessoaAoProjeto(Projeto Projeto, ProjetoDbContext db)
        {
            Console.WriteLine("== Adicionar Pessoa ao Projeto ==");
            Console.Write("Digite o email da pessoa a ser adicionada: ");
            string? emailPessoa = Console.ReadLine();
            
            if (emailPessoa == null){
                Console.WriteLine("Email não pode ser nulo!");
                return;
            }

            var pessoa = db.Usuarios?.FirstOrDefault(u => u.Email == emailPessoa);

            if (pessoa == null)
            {
                Console.WriteLine("Usuário não encontrado.");
                return;
            }
            

            var ProjetoUsuario = new ProjetoUsuario
            {
                Projeto = Projeto,
                Usuario = pessoa,
                UsuarioId = pessoa.UsuarioId
            };

            Projeto.ProjetosUsuarios.Add(ProjetoUsuario);
            db.SaveChanges();

            Console.WriteLine($"Pessoa '{pessoa.Nome}' adicionada ao Projeto '{Projeto.Nome}' com sucesso!");
        }

        public static void AdicionarTarefaAoProjeto(Projeto Projeto, ProjetoDbContext db)
        {
            Console.WriteLine("== Adicionar Tarefa ao Projeto ==");

            Console.Write("Digite o nome da tarefa: ");
            string? nomeTarefa = Console.ReadLine();
            if (nomeTarefa == null || nomeTarefa == ""){
                Console.WriteLine("Digite um Nome válido para a Tarefa!");
                return;
            }
            Console.Write("Digite a descrição: ");
            string? descricaoTarefa = Console.ReadLine();
            if (descricaoTarefa == null || descricaoTarefa == ""){
                Console.WriteLine("Digite um Nome válido para a Tarefa!");
                return;
            }

            var tarefa = new Tarefa
            {
                Nome = nomeTarefa,
                ProjetoId = Projeto.ProjetoId,
                Descricao = descricaoTarefa,
                Prioridade = "Normal",
                Status = "Em Andamento"
            };

            db.Tarefas?.Add(tarefa);
            db.SaveChanges();

            Console.WriteLine("Tarefa adicionada ao Projeto com sucesso!");
        }

        public static void ListarTarefasProjeto(Usuario? Usuario, Projeto? Projeto, ProjetoDbContext db)
        {
            if (Usuario == null || Projeto == null)
            {
                Console.WriteLine("Nenhum usuário ou Projeto selecionado.");
                return;
            }
            Console.WriteLine();
            Console.WriteLine("Tarefas do Projeto:");

            var tarefas = db.Tarefas?
                .Where(t => t.ProjetoId == Projeto.ProjetoId)
                .Select(t => new { t.TarefaId, t.Nome }) // Selecionar apenas as propriedades necessárias
                .ToList();

            if (tarefas?.Count == 0)
            {
                Console.WriteLine("Nenhuma tarefa encontrada.");
            }
            else
            {
                for (int i = 0; i < tarefas?.Count; i++)
                {
                    var tarefa = tarefas[i];
                    Console.WriteLine($"ID: {tarefa.TarefaId}, Nome: {tarefa.Nome}");
                }
            }
        }

        

        public static bool IsGerenteOuSuperior(Usuario Usuario, Projeto Projeto, ProjetoDbContext db)
        {
            var ProjetoUsuario = db.ProjetosUsuarios?.FirstOrDefault(pu => pu.ProjetoId == Projeto.ProjetoId && pu.UsuarioId == Usuario.UsuarioId);

            return ProjetoUsuario != null && ProjetoUsuario.IsGerente;
        }

        public static bool IsGerenteOuSuperiorTarefa(Usuario Usuario, Tarefa Tarefa, ProjetoDbContext db)
        {
            var TarefaUsuario = db.TarefasUsuarios?.FirstOrDefault(pu => pu.TarefaId == Tarefa.TarefaId && pu.UsuarioId == Usuario.UsuarioId);

            return TarefaUsuario != null && TarefaUsuario.IsGerente;
        }

        public static void AdicionarPrioridade(Tarefa Tarefa, ProjetoDbContext db)
        {
            Console.WriteLine("== Adicionar Prioridade ao Projeto ==");
            Console.Write("Digite a Prioridade do Projeto (Alta/Normal/Baixa): ");
            string? novaPrioridade = Console.ReadLine();
            
            if (novaPrioridade == "Alta" || novaPrioridade == "Normal" || novaPrioridade == "Baixa")
            {
                Tarefa.Prioridade = novaPrioridade;
                db.SaveChanges();
                Console.WriteLine("Status do Projeto alterado com sucesso!");
            }
            else
            {
                Console.WriteLine("Prioridade inválida.");
            }
        }
    }
}