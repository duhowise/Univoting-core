using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Akka.Hosting;
using Akka.Persistence.Hosting;
using Akka.Actor;
using Akka.Persistence.Sql.Hosting;
using Univoting.Akka.Actors;
using Univoting.Akka.Messages;
using Univoting.Akka.Utility;
using Univoting.Models;

// ReSharper disable once IdentifierTypo
namespace Univoting.Akka;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddAkka("VotingSystem", (builder, provider) =>
                {
                    builder
                        .WithActors((system, registry) =>
                        {
                            var supervisor = system.ActorOf(
                                Props.Create(() => new VotingSupervisorActor()),
                                "voting-supervisor");
                            registry.Register<VotingSupervisorActor>(supervisor);
                        })
                        .WithSqlPersistence(
                            journal =>
                            {
                                journal.AutoInitialize = true;
                                journal.ConnectionString = context.Configuration.GetConnectionString("DefaultConnection")!;
                                journal.DatabaseOptions = JournalDatabaseOptions.Sqlite;
                                journal.ProviderName = "Microsoft.Data.Sqlite";
                            },
                            snapshot =>
                            {
                                snapshot.AutoInitialize = true;
                                snapshot.ConnectionString = context.Configuration.GetConnectionString("DefaultConnection")!;
                                snapshot.DatabaseOptions = SnapshotDatabaseOptions.Sqlite;
                                snapshot.ProviderName = "Microsoft.Data.Sqlite";
                            });
                });
            })
            .Build();

        using (host)
        {
            await host.StartAsync();
            
            // Initialize database
            SqliteEventStore.InitializeDatabase();

            // Get actor references from DI
            var requiredActor = host.Services.GetRequiredService<IRequiredActor<VotingSupervisorActor>>();
            var supervisor = requiredActor.ActorRef;

            // Demo the voting system
            // Console.WriteLine("Choose demo type:");
            // Console.WriteLine("1. Basic Demo (original)");
            // Console.WriteLine("2. Enhanced Demo (comprehensive)");
            // Console.Write("Enter choice (1 or 2): ");
            //
            // var choice = Console.ReadLine();
            // if (choice == "2")
            // {
            //     await EnhancedVotingDemo.RunEnhancedDemo(supervisor);
            // }
            // else
            // {
                await DemoVotingSystem(supervisor);
            // }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            
            await host.StopAsync();
        }
    }

    private static async Task DemoVotingSystem(IActorRef supervisor)
    {
        try
        {
            var electionId = Guid.NewGuid().ToString();
            
            Console.WriteLine("=== Univoting Akka.NET Demo ===");
            
            // 1. Create Election (idempotent)
            Console.WriteLine("1. Creating election...");
            
            // Check if election already exists
            var existingElection = await supervisor.Ask<Status>(
                new GetElection(electionId));
            
            if (existingElection is not Status.Failure)
            {
                Console.WriteLine("✓ Election already exists, skipping creation");
            }
            else
            {
                var createResult = await supervisor.Ask<object>(
                    new CreateElection(electionId, "University Student Elections 2024", 
                        "Annual student body elections", null, "#0066CC"), TimeSpan.FromSeconds(10));
                
                if (createResult is Status.Success)
                {
                    Console.WriteLine("✓ Election created successfully");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to create election: {createResult}");
                    return;
                }
            }

            // 2. Add Positions (idempotent)
            Console.WriteLine("\n2. Adding positions...");
            var positions = new[]
            {
                new { Id = Guid.NewGuid().ToString(), Name = "Student Body President", Priority = 1 },
                new { Id = Guid.NewGuid().ToString(), Name = "Vice President", Priority = 2 },
                new { Id = Guid.NewGuid().ToString(), Name = "Secretary", Priority = 3 }
            };

            foreach (var pos in positions)
            {
                // Check if position already exists
                var existingPosition = await supervisor.Ask<object>(
                    new GetPosition(pos.Id, electionId), TimeSpan.FromSeconds(5));
                
                if (existingPosition is not Status.Failure)
                {
                    Console.WriteLine($"✓ Position already exists: {pos.Name}");
                    continue;
                }
                
                var posResult = await supervisor.Ask<object>(
                    new AddPosition(electionId, pos.Id, pos.Name, pos.Priority), 
                    TimeSpan.FromSeconds(5));
                
                if (posResult is Status.Success)
                {
                    Console.WriteLine($"✓ Added position: {pos.Name}");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to add position {pos.Name}: {posResult}");
                }
            }

            // 3. Add Candidates (idempotent)
            Console.WriteLine("\n3. Adding candidates...");
            var candidates = new[]
            {
                new { PositionId = positions[0].Id, Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe", Priority = 1 },
                new { PositionId = positions[0].Id, Id = Guid.NewGuid().ToString(), FirstName = "Jane", LastName = "Smith", Priority = 2 },
                new { PositionId = positions[1].Id, Id = Guid.NewGuid().ToString(), FirstName = "Bob", LastName = "Johnson", Priority = 1 },
                new { PositionId = positions[1].Id, Id = Guid.NewGuid().ToString(), FirstName = "Alice", LastName = "Wilson", Priority = 2 }
            };

            foreach (var candidate in candidates)
            {
                // Check if candidate already exists
                var existingCandidate = await supervisor.Ask<object>(
                    new GetCandidate(candidate.Id), TimeSpan.FromSeconds(5));
                
                if (existingCandidate is not Status.Failure)
                {
                    Console.WriteLine($"✓ Candidate already exists: {candidate.FirstName} {candidate.LastName}");
                    continue;
                }
                
                var candResult = await supervisor.Ask<object>(
                    new AddCandidate(candidate.PositionId, candidate.Id, candidate.FirstName, 
                        candidate.LastName, null, candidate.Priority, electionId), 
                    TimeSpan.FromSeconds(5));
                
                if (candResult is Status.Success)
                {
                    Console.WriteLine($"✓ Added candidate: {candidate.FirstName} {candidate.LastName}");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to add candidate {candidate.FirstName} {candidate.LastName}: {candResult}");
                }
            }

            // 4. Register Voters (idempotent)
            Console.WriteLine("\n4. Registering voters...");
            var voters = new[]
            {
                new { Id = Guid.NewGuid().ToString(), Name = "Student A", IdNumber = "STU001" },
                new { Id = Guid.NewGuid().ToString(), Name = "Student B", IdNumber = "STU002" },
                new { Id = Guid.NewGuid().ToString(), Name = "Student C", IdNumber = "STU003" }
            };

            foreach (var voter in voters)
            {
                // Check if voter already exists
                var existingVoter = await supervisor.Ask<object>(
                    new GetVoter(voter.Id), TimeSpan.FromSeconds(5));
                
                if (existingVoter is not Status.Failure)
                {
                    Console.WriteLine($"✓ Voter already registered: {voter.Name}");
                    continue;
                }
                
                var voterResult = await supervisor.Ask<object>(
                    new RegisterVoter(electionId, voter.Id, voter.Name, voter.IdNumber), 
                    TimeSpan.FromSeconds(5));
                
                if (voterResult is Status.Success)
                {
                    Console.WriteLine($"✓ Registered voter: {voter.Name}");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to register voter {voter.Name}: {voterResult}");
                }
            }

            // 5. Cast some votes
            Console.WriteLine("\n5. Casting votes...");
            var votes = new[]
            {
                new { VoterId = voters[0].Id, CandidateId = candidates[0].Id, PositionId = positions[0].Id },
                new { VoterId = voters[1].Id, CandidateId = candidates[1].Id, PositionId = positions[0].Id },
                new { VoterId = voters[0].Id, CandidateId = candidates[2].Id, PositionId = positions[1].Id },
                new { VoterId = voters[1].Id, CandidateId = candidates[3].Id, PositionId = positions[1].Id }
            };

            foreach (var vote in votes)
            {
                var voteResult = await supervisor.Ask<object>(
                    new CastVote(vote.VoterId, vote.CandidateId, vote.PositionId, electionId), 
                    TimeSpan.FromSeconds(5));
                
                if (voteResult is Status.Success)
                {
                    Console.WriteLine($"✓ Vote cast by {vote.VoterId} for {vote.CandidateId}");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to cast vote: {voteResult}");
                }
            }

            // 6. Skip a vote
            Console.WriteLine("\n6. Skipping a vote...");
            var skipResult = await supervisor.Ask<object>(
                new SkipVote(voters[2].Id, positions[0].Id, electionId), 
                TimeSpan.FromSeconds(5));
            
            if (skipResult is Status.Success)
            {
                Console.WriteLine($"✓ Vote skipped by voter3 for president position");
            }
            else
            {
                Console.WriteLine($"✗ Failed to skip vote: {skipResult}");
            }

            // 7. Get vote counts
            Console.WriteLine("\n7. Getting vote counts...");
            foreach (var pos in positions)
            {
                var voteCount = await supervisor.Ask<int>(
                    new GetVoteCount(pos.Id, electionId), TimeSpan.FromSeconds(5));
                
                var skippedCount = await supervisor.Ask<int>(
                    new GetSkippedVoteCount(pos.Id, electionId), TimeSpan.FromSeconds(5));
                
                Console.WriteLine($"Position: {pos.Name} - Votes: {voteCount}, Skipped: {skippedCount}");
            }

            // 8. Get election data
            Console.WriteLine("\n8. Getting election information...");
            var election = await supervisor.Ask<Election>(
                new GetElection(electionId), TimeSpan.FromSeconds(5));
            
            Console.WriteLine($"Election: {election.Name} - {election.Description}");

            // 9. Get positions for election
            var electionPositions = await supervisor.Ask<List<Position>>(
                new GetPositionsForElection(electionId), TimeSpan.FromSeconds(5));
            
            Console.WriteLine($"Total positions in election: {electionPositions.Count}");

            Console.WriteLine("\n=== Demo completed successfully! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Demo failed with error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
