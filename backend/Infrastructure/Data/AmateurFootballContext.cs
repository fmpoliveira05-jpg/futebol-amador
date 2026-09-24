using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <summary>
    /// Classe de Contexto de Base de Dados principal para a aplicação Amateur Football.
    /// 
    /// Esta classe herda de [DbContext] e funciona como uma unidade de trabalho (Unit of Work) ou
    /// sessão de trabalho com a base de dados relacional.
    /// É o ponto central onde as Entidades de Domínio são mapeadas para tabelas da base de dados.
    /// </summary>
    public class AmateurFootballContext : DbContext
    {
        /// <summary> Tabela para a classe base de Utilizadores. </summary>
        public DbSet<User> User { get; set; } = null!;

        /// <summary> Tabela para perfis de Jogadores. </summary>
        public DbSet<Player> Player { get; set; } = null!;

        /// <summary> Tabela para utilizadores com privilégios de Super Administrador. </summary>
        public DbSet<SuperAdmin> SuperAdmin { get; set; } = null!;

        /// <summary> Tabela para as Equipas. </summary>
        public DbSet<Team> Team { get; set; } = null!;

        /// <summary> Tabela para os níveis de Classificação (Ranks). </summary>
        public DbSet<Rank> Rank { get; set; } = null!;

        /// <summary> Tabela para os Campos de Jogo (Pitches). </summary>
        public DbSet<Pitch> Pitch { get; set; } = null!;

        /// <summary> Tabela para os Calendários de Partidas das Equipas. </summary>
        public DbSet<Calendar> Calendar { get; set; } = null!;

        /// <summary> Tabela para as Partidas Agendadas. </summary>
        public DbSet<Matches> Match { get; set; } = null!;

        /// <summary> Tabela para o registo de Pedidos de Adiamento. </summary>
        public DbSet<PostPoneMatch> PostPoneMatch { get; set; } = null!;

        /// <summary> Tabela para o registo de Partidas Canceladas. </summary>
        public DbSet<CancelledMatch> CancelledMatch { get; set; } = null!;

        /// <summary> Tabela para as Estatísticas de Partida por Equipa. </summary>
        public DbSet<TeamStatistics> TeamStatistics { get; set; } = null!;

        /// <summary> Tabela para o conteúdo das Mensagens (Deprecated - Ver Firestore). </summary>
        public DbSet<Message> Message { get; set; } = null!;

        /// <summary> Tabela para as Salas de Chat. </summary>
        public DbSet<Chat> Chat { get; set; } = null!;

        /// <summary> Tabela para o registo de Convites de Partida. </summary>
        public DbSet<MatchInvite> MatchInvite { get; set; } = null!;

        /// <summary> Tabela para o registo de Pedidos de Adesão. </summary>
        public DbSet<MembershipRequest> MembershipRequests { get; set; } = null!;

        /// <summary>
        /// Construtor principal.
        /// Recebe as opções de configuração (ConnectionString, Provider) através da Injeção de Dependências.
        /// </summary>
        /// <param name="options">As opções de contexto a serem passadas à classe base [DbContext].</param>
        public AmateurFootballContext(DbContextOptions<AmateurFootballContext> options)
        : base(options)
        {
        }

        /// <summary>
        /// Configuração do modelo de dados, relações e comportamentos.
        /// Este método é onde o EF Core é configurado para lidar com chaves estrangeiras, herança e regras de deleção.
        /// </summary>
        /// <param name="modelBuilder">O construtor do modelo de dados.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 PostPoneMatch → Team
            modelBuilder.Entity<PostPoneMatch>()
                .HasOne(p => p.Team)
                .WithMany()
                .HasForeignKey(p => p.IdTeamPostPone)
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 PostPoneMatch → Match
            modelBuilder.Entity<PostPoneMatch>()
                .HasOne(p => p.Match)
                .WithMany()
                .HasForeignKey(p => p.IdMatch)
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 CancelledMatch → Team
            modelBuilder.Entity<CancelledMatch>()
                .HasOne(p => p.Team)
                .WithMany()
                .HasForeignKey(p => p.IdTeam)
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 CancelledMatch → Match
            modelBuilder.Entity<CancelledMatch>()
                .HasOne(p => p.Match)
                .WithMany()
                .HasForeignKey(p => p.IdMatch)
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 TeamStatistics → Match
            modelBuilder.Entity<TeamStatistics>()
                .HasOne(ts => ts.Match)
                .WithMany(m => m.Teams)
                .HasForeignKey(ts => ts.MatchesId)
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 MatchInvite → Sender (Team)
            modelBuilder.Entity<MatchInvite>()
                .HasOne(mi => mi.Sender)
                .WithMany(t => t.SentInvites)
                .HasForeignKey(mi => mi.IdSender)
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 MatchInvite → Receiver (Team)
            modelBuilder.Entity<MatchInvite>()
                .HasOne(mi => mi.Receiver)
                .WithMany(t => t.ReceivedInvites)
                .HasForeignKey(mi => mi.IdReceiver)
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 Matches → Pitch
            modelBuilder.Entity<Matches>()
                .HasOne(m => m.Pitch)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 Matches → Chat
            modelBuilder.Entity<Matches>()
                .HasOne(m => m.Chat)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 Team → Calendar
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Calendar)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            // 🔹 Rank → NextRank
            modelBuilder.Entity<Rank>()
                .HasOne(rank => rank.NextRank)
                .WithOne()
                .HasForeignKey<Rank>(rank => rank.IdNextRank)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Rank → PreviousRank
            modelBuilder.Entity<Rank>()
                .HasOne(rank => rank.PreviousRank)
                .WithOne()
                .HasForeignKey<Rank>(rank => rank.IdPreviousRank)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Herança — TPT
            modelBuilder.Entity<User>()
                .UseTptMappingStrategy()
                .ToTable("User");
        }
    }
}