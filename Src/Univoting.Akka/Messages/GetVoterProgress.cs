namespace Univoting.Akka.Messages;

public record GetVoterProgress(string VoterId, int TotalPositionsInElection) : VotingCommand;