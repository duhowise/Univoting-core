using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Akka.Hosting;
using Akka.Persistence.Hosting;
using Akka.Actor;
using Akka.Persistence.Sql.Hosting;
using Univoting.Akka.Actors;
using Univoting.Akka.Messages;
using Univoting.Akka.Models;
using Univoting.Akka.Utility;


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
            
            Console.WriteLine("Demo completed. Shutting down in 2 seconds...");
            await Task.Delay(2000);
            
            await host.StopAsync();
        }
    }

    private static async Task DemoVotingSystem(IActorRef supervisor)
    {
        try
        {
            var electionId = Guid.Parse("935f39e2-177c-4d1f-8d4d-7e1a2458e09e");

            // Console.WriteLine("=== Univoting Akka.NET Demo ===");

            // // 1. Create Election (idempotent)
            // Console.WriteLine("1. Creating election...");

            // // Check if election already exists
            // try
            // {
            //     var existingElection = await supervisor.Ask<Election>(
            //         new GetElection(electionId), TimeSpan.FromSeconds(5));
            //     Console.WriteLine("✓ Election already exists, skipping creation");
            // }
            // catch (Exception)
            // {
            //     // Election doesn't exist, create it
            //     var createResult = await supervisor.Ask<object>(
            //         new CreateElection(electionId, "University Student Elections 2024", 
            //             "Annual student body elections", null, "#0066CC"), TimeSpan.FromSeconds(10));

            //     if (createResult is Status.Success)
            //     {
            //         Console.WriteLine("✓ Election created successfully");
            //     }
            //     else
            //     {
            //         Console.WriteLine($"✗ Failed to create election: {createResult}");
            //         return;
            //     }
            // }
            
            //request election data
            var election = await supervisor.Ask<Election>(
                new GetElection(electionId), TimeSpan.FromSeconds(5));
            Console.WriteLine($"Election: {election.Name} - {election.Description}");
    var summary = await supervisor.Ask<ElectionVotesSummary>(
                new GetAllVotesForElection(electionId), TimeSpan.FromSeconds(5));
            Console.WriteLine($"Election Summary: {summary.ElectionName} - Total Votes: {summary.TotalVotes}, Skipped Votes: {summary.TotalSkippedVotes}, Total Participation: {summary.TotalVotes + summary.TotalSkippedVotes}");
            summary.PositionVotes.ForEach(pos =>
            {
                Console.WriteLine($"Position: {pos.PositionName} - Votes: {pos.TotalVotesForPosition}, Skipped: {pos.SkippedVotesForPosition}, Total Participation: {pos.TotalParticipationForPosition}");
                pos.CandidateVotes.ForEach(cand =>
                {
                    Console.WriteLine($"  Candidate: {cand.CandidateName} - Votes: {cand.VoteCount}");
                });
            });




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
                try
                {
                    var existingPosition = await supervisor.Ask<Position>(
                        new GetPosition(pos.Id, electionId), TimeSpan.FromSeconds(5));
                    Console.WriteLine($"✓ Position already exists: {pos.Name}");
                    continue;
                }
                catch (Exception)
                {
                    // Position doesn't exist, add it
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
                try
                {
                    var existingCandidate = await supervisor.Ask<Candidate>(
                        new GetCandidate(candidate.Id, electionId), TimeSpan.FromSeconds(5));
                    Console.WriteLine($"✓ Candidate already exists: {candidate.FirstName} {candidate.LastName}");
                    continue;
                }
                catch (Exception)
                {
                    // Candidate doesn't exist, add it
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
                try
                {
                    var existingVoter = await supervisor.Ask<Voter>(
                        new GetVoter(voter.Id, electionId), TimeSpan.FromSeconds(5));
                    Console.WriteLine($"✓ Voter already registered: {voter.Name}");
                    continue;
                }
                catch (Exception)
                {
                    // Voter doesn't exist, register them
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
            Console.WriteLine("\n8. Getting election data...");

            // 10. Get all votes summary
            Console.WriteLine("\n10. Getting all votes summary...");
            var votesSummary = await supervisor.Ask<ElectionVotesSummary>(
                new GetAllVotesForElection(electionId), TimeSpan.FromSeconds(5));
            
            Console.WriteLine($"\n=== ELECTION RESULTS: {votesSummary.ElectionName} ===");
            Console.WriteLine($"Total Votes Cast: {votesSummary.TotalVotes}");
            Console.WriteLine($"Total Votes Skipped: {votesSummary.TotalSkippedVotes}");
            Console.WriteLine($"Total Participation: {votesSummary.TotalVotes + votesSummary.TotalSkippedVotes}");
            
            foreach (var position in votesSummary.PositionVotes)
            {
                Console.WriteLine($"\n--- {position.PositionName} ---");
                Console.WriteLine($"Votes Cast: {position.TotalVotesForPosition}, Skipped: {position.SkippedVotesForPosition}, Total Participation: {position.TotalParticipationForPosition}");
                
                if (position.CandidateVotes.Any())
                {
                    Console.WriteLine("Results:");
                    for (int i = 0; i < position.CandidateVotes.Count; i++)
                    {
                        var candidate = position.CandidateVotes[i];
                        var rank = i + 1;
                        var suffix = rank switch
                        {
                            1 => "st",
                            2 => "nd", 
                            3 => "rd",
                            _ => "th"
                        };
                        Console.WriteLine($"  {rank}{suffix}: {candidate.CandidateName} - {candidate.VoteCount} votes");
                    }
                }
                else
                {
                    Console.WriteLine("  No votes cast for this position");
                }
            }

            Console.WriteLine("\n=== Demo completed successfully! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Demo failed with error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
