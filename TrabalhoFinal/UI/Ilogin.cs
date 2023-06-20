using System.Runtime.CompilerServices;
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
    public class LoginUI
    {
        public static Usuario? RealizarLogin(ProjetoDbContext db)
        {
            Console.WriteLine("== Login ==");

            Console.Write("Digite seu email: ");
            string? email = Console.ReadLine();

            Console.Write("Digite sua senha: ");
            string? senha = Console.ReadLine();

            var usuario = db.Usuarios?.FirstOrDefault(u => u.Email == email && u.Senha == senha);

            if (usuario == null)
            {
                Console.WriteLine("Email ou senha inválidos.");
                return null;
            }

            Console.WriteLine("Login realizado com sucesso!");

            return usuario;
        }

        public static Usuario? Registrar(ProjetoDbContext db)
        {
            Console.WriteLine("== Registrar Usuário ==");

            Console.Write("Digite seu nome: ");
            string? nome = Console.ReadLine();

            Console.Write("Digite seu email: ");
            string? email = Console.ReadLine();

            Console.Write("Digite sua senha: ");
            string? senha = Console.ReadLine();

            try
            {
                if (nome == null)
                {
                    throw new ArgumentNullException(nameof(nome), "Não foi possível fazer o cadastro porque o nome está nulo!");
                }

                if (email == null)
                {
                    throw new ArgumentNullException(nameof(email), "Não foi possível fazer o cadastro porque o email está nulo!");
                }

                if (senha == null)
                {
                    throw new ArgumentNullException(nameof(senha), "Não foi possível fazer o cadastro porque a senha está nula!");
                }

                var emails = db.Usuarios?.Where(t => t.Email == email).ToList();
                if (emails?.Count != 0)
                {
                    throw new InvalidOperationException("Email já cadastrado!");
                }

                Usuario usuario = new Usuario
                {
                    Nome = nome,
                    Email = email,
                    Senha = senha
                };

                db.Usuarios?.Add(usuario);
                db.SaveChanges();

                Console.WriteLine("Usuário registrado com sucesso!");

                return usuario;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro durante o cadastro do usuário: " + ex.Message);
                return null;
            }
        }
    }
}
