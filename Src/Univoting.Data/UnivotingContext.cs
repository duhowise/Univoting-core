using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Univoting.Models;

namespace Univoting.Data
{
    public class UnivotingContext:DbContext
    {

        public UnivotingContext(DbContextOptions<UnivotingContext> contextOptions):base(contextOptions)
        {
            
        }

        #region ManualConfig

        //private readonly IConfigurationRoot _config;

        //public UnivotingContext()
        //{
        //   _config=new ConfigurationBuilder()
        //       .SetBasePath(Directory.GetCurrentDirectory())
        //       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //       .Build();
        //}
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(_config.GetConnectionString(""));
        //}
        

        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (
                var pb in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetProperties()
                        .Where(p => p.ClrType == typeof(string)))
                    .Select(p => modelBuilder.Entity(p.DeclaringEntityType.ClrType).Property(p.Name)))
            {
                pb.IsUnicode(false).HasMaxLength(150);
            }

            modelBuilder.Entity<Candidate>().HasOne(x => x.Priority);
            modelBuilder.Entity<Position>().HasOne(x => x.Priority);
            modelBuilder.Entity<Voter>().HasMany(c => c.SkippedVotes).WithOne(x => x.Voter).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Voter>().HasMany(c => c.Votes).WithOne(x => x.Voter).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Vote>().HasOne(c => c.Position).WithMany(x=>x.Votes).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Moderator>().Property(x => x.Badge).HasConversion(new EnumToStringConverter<Badge>());
            modelBuilder.Entity<Voter>().Property(x => x.VotingStatus).HasConversion(new EnumToStringConverter<VotingStatus>());
        }

        public  DbSet<Vote> Votes { get; set; }
        public  DbSet<Voter> Voters { get; set; }
        public  DbSet<Candidate> Candidates { get; set; }
        public  DbSet<Position> Positions { get; set; }
        public  DbSet<Priority> Priorities { get; set; }
        public  DbSet<SkippedVote> SkippedVotes { get; set; }
        public  DbSet<Election> Elections { get; set; }
        public  DbSet<Moderator> Moderators { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<PollingStation> PollingStations { get; set; }
    }
}
