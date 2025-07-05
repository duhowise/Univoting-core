namespace Univoting.Akka.Models;

// Voter-related result classes
public class VoterEligibilityResult
{
    public string VoterId { get; }
    public bool IsEligible { get; }
    public string Reason { get; }

    public VoterEligibilityResult(string voterId, bool isEligible, string reason)
    {
        VoterId = voterId;
        IsEligible = isEligible;
        Reason = reason;
    }
}

// Position-related result classes

// Election-wide statistics classes