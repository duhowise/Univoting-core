

namespace Univoting.Akka.Models;

public class ElectionVotesSummary
{
    public Guid ElectionId { get; set; }
    public string ElectionName { get; set; } = string.Empty;
    public List<PositionVotes> PositionVotes { get; set; } = new();
    public int TotalVotes { get; set; }
    public int TotalSkippedVotes { get; set; }
}

public class PositionVotes
{
    public string PositionId { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public List<CandidateVotes> CandidateVotes { get; set; } = new();
    public int TotalVotesForPosition { get; set; } // Actual votes cast for candidates
    public int SkippedVotesForPosition { get; set; } // Votes that were skipped
    public int TotalParticipationForPosition { get; set; } // Total participation (votes + skipped)
}

public class CandidateVotes
{
    public string CandidateId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public List<Vote> Votes { get; set; } = new();
}
