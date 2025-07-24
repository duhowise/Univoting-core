using Akka.Actor;
using Akka.TestKit.Xunit2;
using Univoting.Akka.Actors;
using Univoting.Akka.Actors.MessageExtractors;
using Univoting.Akka.Messages;
using Univoting.Akka.SharedModels;
using Xunit;
using Xunit.Abstractions;

namespace Univoting.Akka.Tests;

public class ElectionActorTests : TestKit
{
    private readonly ITestOutputHelper _output;

    public ElectionActorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private IActorRef CreateElectionsParent() => Sys.ActorOf(GenericChildPerEntityParent.Props(
        new ElectionMessageExtractor(),
        id => Props.Create(() => new ElectionActor(id))
    ));

    [Fact]
    public void ElectionActor_Should_Create_Election_Successfully()
    {
        // Arrange
        var electionId = Guid.NewGuid().ToString();
        var parent = CreateElectionsParent();
        
        // Act & Assert - Create Election
        parent.Tell(new CreateElection(electionId, "Test Election", "Test Description", null, "#000000"));
        ExpectMsg<Status.Success>();
        
        // Verify election was created
        parent.Tell(new GetElection(electionId));
        var election = ExpectMsg<Election>();
        
        Assert.Equal("Test Election", election.Name);
        Assert.Equal("Test Description", election.Description);
        Assert.Equal("#000000", election.BrandColour);
    }

    [Fact]
    public void ElectionActor_Should_Add_Position_And_Candidate()
    {
        // Arrange
        var electionId = Guid.NewGuid().ToString();
        var positionId = Guid.NewGuid().ToString();
        var candidateId = Guid.NewGuid().ToString();
        var parent = CreateElectionsParent();
        
        // Create Election
        parent.Tell(new CreateElection(electionId, "Test Election", "Test Description", null, null));
        ExpectMsg<Status.Success>();
        
        // Act & Assert - Add Position
        parent.Tell(new AddPosition(electionId, positionId, "President", 1));
        ExpectMsg<Status.Success>();
        
        // Add Candidate
        parent.Tell(new AddCandidate(positionId, candidateId, "John", "Doe", null, 1,electionId));
        ExpectMsg<Status.Success>();
        
        // Verify candidates for position
        parent.Tell(new GetCandidatesForPosition(positionId, electionId));
        var candidates = ExpectMsg<List<Candidate>>();
        
        Assert.Single(candidates, electionId);
        Assert.Equal("John", candidates[0].FirstName);
        Assert.Equal("Doe", candidates[0].LastName);
    }

    [Fact]
    public void ElectionActor_Should_Register_Voter_And_Cast_Vote()
    {
        // Arrange
        var electionId = Guid.NewGuid().ToString();
        var positionId = Guid.NewGuid().ToString();
        var candidateId = Guid.NewGuid().ToString();
        var voterId = Guid.NewGuid().ToString();
        var parent = CreateElectionsParent();
        
        // Setup election, position, candidate
        parent.Tell(new CreateElection(electionId, "Test Election", "Test Description", null, null));
        ExpectMsg<Status.Success>();
        
        parent.Tell(new AddPosition(electionId, positionId, "President", 1));
        ExpectMsg<Status.Success>();
        
        parent.Tell(new AddCandidate(positionId, candidateId, "John", "Doe", null, 1, electionId));
        ExpectMsg<Status.Success>();
        
        // Act & Assert - Register Voter
        parent.Tell(new RegisterVoter(electionId, voterId, "Test Voter", "ID123"));
        ExpectMsg<Status.Success>();
        
        // Cast Vote
        parent.Tell(new CastVote(voterId, candidateId, positionId, electionId));
        ExpectMsg<Status.Success>();
        
        // Verify vote count
        parent.Tell(new GetVoteCount(positionId, electionId));
        var voteCount = ExpectMsg<int>();
        Assert.Equal(1, voteCount);
    }

    [Fact]
    public void ElectionActor_Should_Skip_Vote_Successfully()
    {
        // Arrange
        var electionId = Guid.NewGuid().ToString();
        var positionId = Guid.NewGuid().ToString();
        var voterId = Guid.NewGuid().ToString();
        var parent = CreateElectionsParent();
        
        // Setup election, position, voter
        parent.Tell(new CreateElection(electionId, "Test Election", "Test Description", null, null));
        ExpectMsg<Status.Success>();
        
        parent.Tell(new AddPosition(electionId, positionId, "President", 1));
        ExpectMsg<Status.Success>();
        
        parent.Tell(new RegisterVoter(electionId, voterId, "Test Voter", "ID123"));
        ExpectMsg<Status.Success>();
        
        // Act & Assert - Skip Vote
        parent.Tell(new SkipVote(voterId, positionId, electionId));
        ExpectMsg<Status.Success>();
        
        // Verify skipped vote count
        parent.Tell(new GetSkippedVoteCount(positionId, electionId));
        var skippedCount = ExpectMsg<int>();
        Assert.Equal(1, skippedCount);
    }

    [Fact]
    public void ElectionActor_Should_Prevent_Duplicate_Votes()
    {
        // Arrange
        var electionId = Guid.NewGuid().ToString();
        var positionId = Guid.NewGuid().ToString();
        var candidateId = Guid.NewGuid().ToString();
        var voterId = Guid.NewGuid().ToString();
        var parent = CreateElectionsParent();
        
        // Setup election, position, candidate, voter
        parent.Tell(new CreateElection(electionId, "Test Election", "Test Description", null, null));
        ExpectMsg<Status.Success>();
        
        parent.Tell(new AddPosition(electionId, positionId, "President", 1));
        ExpectMsg<Status.Success>();
        
        parent.Tell(new AddCandidate(positionId, candidateId, "John", "Doe", null, 1, electionId));
        ExpectMsg<Status.Success>();
        
        parent.Tell(new RegisterVoter(electionId, voterId, "Test Voter", "ID123"));
        ExpectMsg<Status.Success>();
        
        // Cast first vote
        parent.Tell(new CastVote(voterId, candidateId, positionId, electionId));
        ExpectMsg<Status.Success>();
        
        // Act & Assert - Try to cast duplicate vote
        parent.Tell(new CastVote(voterId, candidateId, positionId, electionId));
        ExpectNoMsg(TimeSpan.FromSeconds(1)); // Should be ignored due to validation
        
        // Verify vote count is still 1
        parent.Tell(new GetVoteCount(positionId, electionId));
        var voteCount = ExpectMsg<int>();
        Assert.Equal(1, voteCount);
    }

    [Fact]
    public void ElectionActor_Should_Update_Voter_Status()
    {
        // Arrange
        var electionId = Guid.NewGuid().ToString();
        var voterId = Guid.NewGuid().ToString();
        var parent = CreateElectionsParent();
        
        // Setup election and voter
        parent.Tell(new CreateElection(electionId, "Test Election", "Test Description", null, null));
        ExpectMsg<Status.Success>();
        
        parent.Tell(new RegisterVoter(electionId, voterId, "Test Voter", "ID123"));
        ExpectMsg<Status.Success>();
        
        // Act & Assert - Update voter status
        parent.Tell(new UpdateVoterStatus(voterId, VotingStatus.InProgress));
        ExpectMsg<Status.Success>();
        
        // Verify voters for election
        parent.Tell(new GetVotersForElection(electionId));
        var voters = ExpectMsg<List<Voter>>();
        
        Assert.Single(voters, electionId);
        Assert.Equal(VotingStatus.InProgress, voters[0].VotingStatus);
    }

    [Fact]
    public void ElectionActor_Should_Add_Moderator_And_Department()
    {
        // Arrange
        var electionId = Guid.NewGuid().ToString();
        var moderatorId = Guid.NewGuid().ToString();
        var departmentId = Guid.NewGuid().ToString();
        var parent = CreateElectionsParent();
        
        // Setup election
        parent.Tell(new CreateElection(electionId, "Test Election", "Test Description", null, null));
        ExpectMsg<Status.Success>();
        
        // Act & Assert - Add Moderator
        parent.Tell(new AddModerator(electionId, moderatorId, "Chief Moderator", Badge.Chief));
        ExpectMsg<Status.Success>();
        
        // Add Department
        parent.Tell(new AddDepartment(electionId, departmentId, "Computer Science"));
        ExpectMsg<Status.Success>();
        
        // Add Polling Station
        parent.Tell(new AddPollingStation(electionId, Guid.NewGuid().ToString(), "Main Hall"));
        ExpectMsg<Status.Success>();
        
        _output.WriteLine("Successfully added moderator, department, and polling station");
    }
}
