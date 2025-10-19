using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Data
{
    //Esta classe herda de DbContext e funciona como uma sessão de trabalho DB
    public class AmateurFootballContext : DbContext
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
        public DbSet<MembershipRequests> MembershipRequests { get; set; } = null;

        public AmateurFootballContext(DbContextOptions<AmateurFootballContext> options)
        : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Ligação à base de dados
            optionsBuilder.UseSqlServer(
                @"Data Source=.;Initial Catalog=AmateurFootball;Integrated Security=True;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da relação do Sender (quem envia)
            modelBuilder.Entity<MatchInvite>()
                .HasOne(mi => mi.Sender)
                .WithMany(t => t.SentInvites) // <-- Aponta para a lista de convites enviados
                .HasForeignKey(mi => mi.IdSender)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuração da relação do Receiver (quem recebe)
            modelBuilder.Entity<MatchInvite>()
                .HasOne(mi => mi.Receiver)
                .WithMany(t => t.ReceivedInvites) // <-- Aponta para a lista de convites recebidos
                .HasForeignKey(mi => mi.IdReceiver)
                .OnDelete(DeleteBehavior.NoAction);

            // Relação 1: De um Rank para o seu NextRank, usando a FK IdNextRank
            modelBuilder.Entity<Rank>()
                .HasOne(rank => rank.NextRank)
                .WithOne() // Não há propriedade de navegação de volta para ESTA relação
                .HasForeignKey<Rank>(rank => rank.IdNextRank) // A FK é IdNextRank
                .IsRequired(false) // Pode ser nulo
                .OnDelete(DeleteBehavior.Restrict); // Evita apagar em cascata

            // Relação 2: De um Rank para o seu PreviousRank, usando a FK IdPreviousRank
            modelBuilder.Entity<Rank>()
                .HasOne(rank => rank.PreviousRank)
                .WithOne() // Também não há propriedade de navegação de volta para ESTA relação
                .HasForeignKey<Rank>(rank => rank.IdPreviousRank) // A FK é IdPreviousRank
                .IsRequired(false) // Também pode ser nulo
                .OnDelete(DeleteBehavior.Restrict);

            //Herança — TPH: uma tabela Users com discriminator
            modelBuilder.Entity<Users>()
                    .UseTptMappingStrategy();

        }
    }
}