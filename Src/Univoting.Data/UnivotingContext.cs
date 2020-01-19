using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Univoting.Models;

namespace Univoting.Data
{
    public class UnivotingContext:DbContext
    {
        private readonly IConfigurationRoot _config;

        public UnivotingContext()
        {
           _config=new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .Build();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_config.GetConnectionString(""));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (
                var pb in modelBuilder.Model.GetEntityTypes()
                    
                        .Where(p => p.ClrType == typeof(string)))
                    
            {
                
            }
        }

        public  DbSet<Vote> Votes { get; set; }
        public  DbSet<Voter> Voters { get; set; }
        public  DbSet<Candidate> Candidates { get; set; }
        public  DbSet<Position> Positions { get; set; }
        public  DbSet<Rank> Ranks { get; set; }
        public  DbSet<SkippedVote> SkippedVotes { get; set; }
        public  DbSet<Election> Elections { get; set; }
        public  DbSet<Moderator> Moderators { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<PollingStation> PollingStations { get; set; }
    }
}
