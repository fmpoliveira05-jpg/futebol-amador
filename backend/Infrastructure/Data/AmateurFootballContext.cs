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
        public DbSet<PostPoneMatch> PostPoneMatch { get; set; } = null;
        public DbSet<CancelledMatch> CancelledMatch { get; set; } = null;
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
            modelBuilder.Entity<Teams>()
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
            modelBuilder.Entity<Users>()
                .UseTptMappingStrategy();

        }
    }
}