using Akka.Actor;
using Univoting.Akka.Messages;
using Univoting.Akka.Models;
using Univoting.Models;
using static System.Collections.Specialized.BitVector32;

namespace Univoting.Akka;

/// <summary>
/// Enhanced demo showcasing the refactored Akka.NET voting system capabilities
/// </summary>
public static class EnhancedVotingDemo
{
    public static async Task RunEnhancedDemo(IActorRef supervisor)
    {
        try
        {
            var electionId = "university-2024";
            
            Console.WriteLine("=== Enhanced Univoting Akka.NET Demo ===");
            Console.WriteLine("Demonstrating Event Sourcing, Actor Model, and Domain-Driven Design\n");
            
            // 1. Create Election with comprehensive setup
            Console.WriteLine("🗳️  PHASE 1: Election Setup");
            await CreateElectionWithFullSetup(supervisor, electionId);
            
            // 2. Demonstrate voter registration and management
            Console.WriteLine("\n👥 PHASE 2: Voter Management");
            await DemonstrateVoterManagement(supervisor, electionId);
            
            // 3. Show voting process with validation
            Console.WriteLine("\n✅ PHASE 3: Voting Process with Validation");
            await DemonstrateVotingProcess(supervisor, electionId);
            
            // 4. Display real-time statistics and monitoring
            Console.WriteLine("\n📊 PHASE 4: Real-time Statistics & Monitoring");
            await DemonstrateStatisticsAndMonitoring(supervisor, electionId);
            
            // 5. Show business rule enforcement
            Console.WriteLine("\n🛡️  PHASE 5: Business Rule Enforcement");
            await DemonstrateBusinessRules(supervisor, electionId);
            
            // 6. Demonstrate event sourcing capabilities
            Console.WriteLine("\n🕒 PHASE 6: Event Sourcing & Audit Trail");
            await DemonstrateEventSourcing(supervisor, electionId);
            
            Console.WriteLine("\n🎉 === Enhanced Demo Completed Successfully! ===");
            Console.WriteLine("The system demonstrates:");
            Console.WriteLine("✓ Event Sourcing for complete audit trails");
            Console.WriteLine("✓ Actor Model for scalability and isolation");
            Console.WriteLine("✓ Domain-Driven Design with proper boundaries");
            Console.WriteLine("✓ Real-time capabilities and monitoring");
            Console.WriteLine("✓ Business rule enforcement");
            Console.WriteLine("✓ Comprehensive state management");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Demo failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private static async Task CreateElectionWithFullSetup(IActorRef supervisor, string electionId)
    {
        // Create Election
        var createResult = await supervisor.Ask<object>(
            new CreateElection(electionId, "University Student Elections 2024", 
                "Annual student body elections with enhanced monitoring", null, "#2E8B57"),
            TimeSpan.FromSeconds(10));
        
        if (createResult is Status.Success)
            Console.WriteLine("✓ Election created successfully");
        else
            throw new InvalidOperationException($"Failed to create election: {createResult}");

        // Add Positions with different priorities
        var positions = new[]
        {
            new { Id = $"{electionId}-president", Name = "Student Body President", Priority = 1 },
            new { Id = $"{electionId}-vicepresident", Name = "Vice President", Priority = 2 },
            new { Id = $"{electionId}-secretary", Name = "Secretary", Priority = 3 },
            new { Id = $"{electionId}-treasurer", Name = "Treasurer", Priority = 4 }
        };

        foreach (var pos in positions)
        {
            var posResult = await supervisor.Ask<object>(
                new AddPosition(electionId, pos.Id, pos.Name, pos.Priority), 
                TimeSpan.FromSeconds(5));
            
            if (posResult is Status.Success)
                Console.WriteLine($"✓ Added position: {pos.Name}");
        }

        // Add multiple candidates per position
        var candidates = new[]
        {
            // President candidates
            new { PositionId = $"{electionId}-president", Id = "pres-candidate-1", FirstName = "Alexandra", LastName = "Johnson", Priority = 1 },
            new { PositionId = $"{electionId}-president", Id = "pres-candidate-2", FirstName = "Michael", LastName = "Chen", Priority = 2 },
            new { PositionId = $"{electionId}-president", Id = "pres-candidate-3", FirstName = "Sarah", LastName = "Williams", Priority = 3 },
            
            // Vice President candidates
            new { PositionId = $"{electionId}-vicepresident", Id = "vp-candidate-1", FirstName = "David", LastName = "Rodriguez", Priority = 1 },
            new { PositionId = $"{electionId}-vicepresident", Id = "vp-candidate-2", FirstName = "Emily", LastName = "Thompson", Priority = 2 },
            
            // Secretary candidates
            new { PositionId = $"{electionId}-secretary", Id = "sec-candidate-1", FirstName = "James", LastName = "Davis", Priority = 1 },
            new { PositionId = $"{electionId}-secretary", Id = "sec-candidate-2", FirstName = "Lisa", LastName = "Anderson", Priority = 2 },
            
            // Treasurer candidates
            new { PositionId = $"{electionId}-treasurer", Id = "treas-candidate-1", FirstName = "Robert", LastName = "Wilson", Priority = 1 }
        };

        foreach (var candidate in candidates)
        {
            var candResult = await supervisor.Ask<object>(
                new AddCandidate(candidate.PositionId, candidate.Id, candidate.FirstName, 
                    candidate.LastName, null, candidate.Priority, electionId), 
                TimeSpan.FromSeconds(5));
            
            if (candResult is Status.Success)
                Console.WriteLine($"✓ Added candidate: {candidate.FirstName} {candidate.LastName} for {candidate.PositionId}");
        }

        // Add departments and moderators
        var departments = new[] { "Engineering", "Business", "Arts", "Sciences" };
        foreach (var dept in departments)
        {
            await supervisor.Ask<object>(
                new AddDepartment(electionId, $"{electionId}-dept-{dept.ToLower()}", dept),
                TimeSpan.FromSeconds(5));
        }

        var moderators = new[]
        {
            new { Id = $"{electionId}-mod-1", Name = "Dr. Jane Smith", Badge = Badge.Chief },
            new { Id = $"{electionId}-mod-2", Name = "Prof. John Doe", Badge = Badge.Senior },
            new { Id = $"{electionId}-mod-3", Name = "Ms. Alice Brown", Badge = Badge.Supervisor }
        };

        foreach (var mod in moderators)
        {
            await supervisor.Ask<object>(
                new AddModerator(electionId, mod.Id, mod.Name, mod.Badge),
                TimeSpan.FromSeconds(5));
        }

        Console.WriteLine($"✓ Election setup complete with {positions.Length} positions, {candidates.Length} candidates, {departments.Length} departments, and {moderators.Length} moderators");
    }

    private static async Task DemonstrateVoterManagement(IActorRef supervisor, string electionId)
    {
        // Register diverse group of voters
        var voters = new[]
        {
            new { Id = $"{electionId}-voter001", Name = "Alice Johnson", IdNumber = "STU001" },
            new { Id = $"{electionId}-voter002", Name = "Bob Smith", IdNumber = "STU002" },
            new { Id = $"{electionId}-voter003", Name = "Carol Davis", IdNumber = "STU003" },
            new { Id = $"{electionId}-voter004", Name = "David Wilson", IdNumber = "STU004" },
            new { Id = $"{electionId}-voter005", Name = "Eva Brown", IdNumber = "STU005" },
            new { Id = $"{electionId}-voter006", Name = "Frank Miller", IdNumber = "STU006" }
        };

        foreach (var voter in voters)
        {
            var voterResult = await supervisor.Ask<object>(
                new RegisterVoter(electionId, voter.Id, voter.Name, voter.IdNumber), 
                TimeSpan.FromSeconds(5));
            
            if (voterResult is Status.Success)
                Console.WriteLine($"✓ Registered voter: {voter.Name} ({voter.IdNumber})");
        }

        // Demonstrate voter status management
        await supervisor.Ask<object>(
            new UpdateVoterStatus($"{electionId}-voter001", VotingStatus.InProgress),
            TimeSpan.FromSeconds(5));
        Console.WriteLine("✓ Updated voter001 status to InProgress");

        // Check voter eligibility
        var eligibility = await supervisor.Ask<VoterEligibilityResult>(
            new CheckVoterEligibility($"{electionId}-voter001", $"{electionId}-president"),
            TimeSpan.FromSeconds(5));
        
        Console.WriteLine($"✓ Voter001 eligibility for President: {eligibility.IsEligible} - {eligibility.Reason}");
    }

    private static async Task DemonstrateVotingProcess(IActorRef supervisor, string electionId)
    {
        var positions = new[] { "president", "vicepresident", "secretary", "treasurer" };
        var candidateChoices = new Dictionary<string, string>
        {
            ["president"] = "pres-candidate-1",
            ["vicepresident"] = "vp-candidate-2", 
            ["secretary"] = "sec-candidate-1",
            ["treasurer"] = "treas-candidate-1"
        };

        // Simulate different voting patterns
        var votingPatterns = new[]
        {
            new { VoterId = $"{electionId}-voter001", Pattern = "complete" }, // Votes for all positions
            new { VoterId = $"{electionId}-voter002", Pattern = "partial" },  // Votes for some, skips others
            new { VoterId = $"{electionId}-voter003", Pattern = "complete" },
            new { VoterId = $"{electionId}-voter004", Pattern = "skip_first" }, // Skips first position
            new { VoterId = $"{electionId}-voter005", Pattern = "complete" },
            new { VoterId = $"{electionId}-voter006", Pattern = "partial" }
        };

        foreach (var pattern in votingPatterns)
        {
            Console.WriteLine($"\n🗳️  Processing votes for {pattern.VoterId} (Pattern: {pattern.Pattern})");
            
            for (int i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                var positionId = $"{electionId}-{position}";
                
                // Determine action based on pattern
                bool shouldVote = pattern.Pattern switch
                {
                    "complete" => true,
                    "partial" => i < 2, // Only vote for first 2 positions
                    "skip_first" => i > 0, // Skip first position
                    _ => true
                };

                if (shouldVote)
                {
                    // Vary candidate choices
                    var candidateId = position switch
                    {
                        "president" => pattern.VoterId.EndsWith("002") ? "pres-candidate-2" : "pres-candidate-1",
                        "vicepresident" => pattern.VoterId.EndsWith("003") ? "vp-candidate-1" : "vp-candidate-2",
                        "secretary" => "sec-candidate-1",
                        "treasurer" => "treas-candidate-1",
                        _ => candidateChoices[position]
                    };

                    var voteResult = await supervisor.Ask<object>(
                        new CastVote(pattern.VoterId, candidateId, positionId, electionId),
                        TimeSpan.FromSeconds(5));
                    
                    if (voteResult is Status.Success)
                        Console.WriteLine($"  ✓ Vote cast for {position}: {candidateId}");
                    else
                        Console.WriteLine($"  ❌ Vote failed for {position}: {voteResult}");
                }
                else
                {
                    var skipResult = await supervisor.Ask<object>(
                        new SkipVote(pattern.VoterId, positionId, electionId),
                        TimeSpan.FromSeconds(5));
                    
                    if (skipResult is Status.Success)
                        Console.WriteLine($"  ⏭️  Skipped position: {position}");
                    else
                        Console.WriteLine($"  ❌ Skip failed for {position}: {skipResult}");
                }
            }

            // Show voter progress
            var progress = await supervisor.Ask<VoterProgress>(
                new GetVoterProgress(pattern.VoterId, positions.Length),
                TimeSpan.FromSeconds(5));
            
            Console.WriteLine($"  📊 Progress: {progress.CompletedPositions}/{progress.TotalPositions} " +
                            $"({progress.ProgressPercentage:F1}%) - " +
                            $"Voted: {progress.VotedPositions}, Skipped: {progress.SkippedPositions}");
        }
    }

    private static async Task DemonstrateStatisticsAndMonitoring(IActorRef supervisor, string electionId)
    {
        // Get comprehensive election statistics
        var stats = await supervisor.Ask<ElectionStatistics>(
            new GetElectionStatistics(electionId),
            TimeSpan.FromSeconds(10));

        Console.WriteLine($"\n📈 Election Statistics for '{stats.ElectionName}':");
        Console.WriteLine($"   Total Voters: {stats.TotalVoters}");
        Console.WriteLine($"   Total Positions: {stats.TotalPositions}");
        Console.WriteLine($"   Overall Participation Rate: {stats.OverallParticipationRate:F1}%\n");

        Console.WriteLine("📊 Position-by-Position Breakdown:");
        foreach (var pos in stats.PositionStatistics)
        {
            Console.WriteLine($"   {pos.PositionName}:");
            Console.WriteLine($"     Votes: {pos.VoteCount}, Skipped: {pos.SkippedCount}");
            Console.WriteLine($"     Participation: {pos.TotalParticipation}/{stats.TotalVoters} " +
                            $"({(double)pos.TotalParticipation / stats.TotalVoters * 100:F1}%)");
        }

        // Get detailed results for each position
        Console.WriteLine("\n🏆 Detailed Voting Results:");
        var positions = new[] { "president", "vicepresident", "secretary", "treasurer" };
        
        foreach (var position in positions)
        {
            var positionId = $"{electionId}-{position}";
            var results = await supervisor.Ask<PositionVotingResults>(
                new GetVotingResults(positionId, electionId),
                TimeSpan.FromSeconds(5));

            Console.WriteLine($"\n   {results.PositionName}:");
            Console.WriteLine($"   Total Votes: {results.TotalVotes}, Skipped: {results.TotalSkipped}");
            
            if (results.CandidateResults.Any())
            {
                Console.WriteLine("   Candidate Results:");
                foreach (var candidate in results.CandidateResults.Take(3)) // Top 3
                {
                    Console.WriteLine($"     {candidate.CandidateName}: {candidate.VoteCount} votes");
                }
            }
        }
    }

    private static async Task DemonstrateBusinessRules(IActorRef supervisor, string electionId)
    {
        Console.WriteLine("🔒 Testing Business Rule Enforcement:");
        
        // Try to vote twice (should fail)
        Console.WriteLine("\n   Testing double voting prevention...");
        var doubleVoteResult = await supervisor.Ask<object>(
            new CastVote($"{electionId}-voter001", "pres-candidate-2", $"{electionId}-president", electionId),
            TimeSpan.FromSeconds(5));
        
        if (doubleVoteResult is Status.Failure failure)
            Console.WriteLine($"   ✓ Double voting correctly prevented: {failure.Cause.Message}");
        else
            Console.WriteLine($"   ❌ Double voting not prevented!");

        // Try to vote for non-existent candidate (should fail)
        Console.WriteLine("\n   Testing invalid candidate prevention...");
        var invalidCandidateResult = await supervisor.Ask<object>(
            new CastVote($"{electionId}-voter002", "non-existent-candidate", $"{electionId}-treasurer", electionId),
            TimeSpan.FromSeconds(5));
        
        if (invalidCandidateResult is Status.Failure failure2)
            Console.WriteLine($"   ✓ Invalid candidate correctly prevented: {failure2.Cause.Message}");
        else
            Console.WriteLine($"   ❌ Invalid candidate not prevented!");

        // Try to skip after voting (should fail)
        Console.WriteLine("\n   Testing skip after vote prevention...");
        var skipAfterVoteResult = await supervisor.Ask<object>(
            new SkipVote($"{electionId}-voter001", $"{electionId}-president", electionId),
            TimeSpan.FromSeconds(5));
        
        if (skipAfterVoteResult is Status.Failure failure3)
            Console.WriteLine($"   ✓ Skip after vote correctly prevented: {failure3.Cause.Message}");
        else
            Console.WriteLine($"   ❌ Skip after vote not prevented!");

        // Check voter eligibility for already voted position
        Console.WriteLine("\n   Testing eligibility checking...");
        var eligibility = await supervisor.Ask<VoterEligibilityResult>(
            new CheckVoterEligibility($"{electionId}-voter001", $"{electionId}-president"),
            TimeSpan.FromSeconds(5));
        
        Console.WriteLine($"   ✓ Eligibility check: {eligibility.IsEligible} - {eligibility.Reason}");
    }

    private static async Task DemonstrateEventSourcing(IActorRef supervisor, string electionId)
    {
        Console.WriteLine("📚 Event Sourcing & Audit Trail Capabilities:");
        
        // Get voter voting history (demonstrates event sourcing)
        var voterHistory = await supervisor.Ask<VoterVotingHistory>(
            new GetVoterHistory($"{electionId}-voter001"),
            TimeSpan.FromSeconds(5));

        Console.WriteLine($"\n   Voting History for {voterHistory.VoterName}:");
        Console.WriteLine($"   Current Status: {voterHistory.Status}");
        Console.WriteLine($"   Positions Voted: {voterHistory.VotedPositions.Count}");
        Console.WriteLine($"   Positions Skipped: {voterHistory.SkippedPositions.Count}");
        
        Console.WriteLine("   Timeline:");
        foreach (var timestamp in voterHistory.VotingTimestamps.OrderBy(t => t.Value))
        {
            var action = voterHistory.VotedPositions.Contains(timestamp.Key) ? "VOTED" : "SKIPPED";
            Console.WriteLine($"     {timestamp.Value:yyyy-MM-dd HH:mm:ss} - {action} for {timestamp.Key}");
        }

        Console.WriteLine("\n   ✓ Complete audit trail maintained through event sourcing");
        Console.WriteLine("   ✓ All state changes are recorded as immutable events");
        Console.WriteLine("   ✓ Point-in-time recovery possible for any moment");
        Console.WriteLine("   ✓ Regulatory compliance and transparency ensured");
    }
}
