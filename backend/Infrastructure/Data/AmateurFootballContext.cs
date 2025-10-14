using Microsoft.EntityFrameworkCore;
using OnStore1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnStore1.Data
{
    //Esta classe herda de DbContext e funciona como uma sessão de trabalho DB
    internal class AmateurFootballContext : DbContext
    {
        public DbSet<Users> User { get; set; } = null;
        public DbSet<Player> Player { get; set; } = null;
        public DbSet<SuperAdmin> SuperAdmin { get; set; } = null;

        public DbSet<Teams> Team { get; set; } = null;
        public DbSet<Rank> Rank { get; set; } = null;
        public DbSet<Pitch> Pitch { get; set; } = null;
        public DbSet<Calendar> Calendar { get; set; } = null;
        public DbSet<Matches> Match { get; set; } = null;
        public DbSet<TeamStatistics> TeamStatistics { get; set; } = null;

        public DbSet<Message> Message { get; set; } = null;
        public DbSet<Chat> Chat { get; set; } = null;
       
        public DbSet<MatchInvite> MatchInvite { get; set; } = null;
        public DbSet<MemberShipRequests> MemberShipRequests { get; set; } = null;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Ligação à base de dados
            optionsBuilder.UseSqlServer(
                @"Data Source=.;Initial Catalog=OnStore1;Integrated Security=True;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Chave composta para MatchInvite
            modelBuilder.Entity<MatchInvite>()
                .HasKey(mi => new { mi.IdSender, mi.IdReceiver });

            //Herança — TPH: uma tabela Users com discriminator
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Player>("Player")
                .HasValue<SuperAdmin>("SuperAdmin");
        }

    }
}