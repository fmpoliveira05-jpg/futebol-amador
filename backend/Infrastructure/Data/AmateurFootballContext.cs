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

            // Configuração da relação hierárquica de Ranks
            modelBuilder.Entity<Rank>()
                .HasOne(r => r.NextRank) // Um Rank tem um (ou nenhum) NextRank
                .WithOne(r => r.PreviousRank) // E esse NextRank tem um PreviousRank (o Rank original)
                .HasForeignKey<Rank>(r => r.IdNextRank) // A chave estrangeira está na propriedade NextRankId do Rank dependente
                .IsRequired(false) // A chave estrangeira não é obrigatória (o último rank não tem próximo)
                .OnDelete(DeleteBehavior.Restrict); // Evita eliminação em cascata

            //Herança — TPH: uma tabela Users com discriminator
            modelBuilder.Entity<Users>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Player>("Player")
                .HasValue<SuperAdmin>("SuperAdmin");
        }
    }
}