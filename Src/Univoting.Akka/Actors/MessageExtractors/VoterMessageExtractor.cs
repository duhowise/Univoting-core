using Univoting.Akka.Messages;
using Univoting.Akka.Utility;

namespace Univoting.Akka.Actors.MessageExtractors;

public class VoterMessageExtractor : EntityMessageExtractor
{
    public VoterMessageExtractor() : base(
        ExtractEntityId,
        ExtractEntityMessage,
        "voter-shard")
    {
    }

    private new static string? ExtractEntityId(object message)
    {
        return message switch
        {
            RegisterVoter regVoter => regVoter.VoterId,
            UpdateVoterStatus updateStatus => updateStatus.VoterId,
            GetVoter getVoter => getVoter.VoterId,
            CheckVoterEligibility checkEligibility => checkEligibility.VoterId,
            GetVoterHistory getHistory => getHistory.VoterId,
            GetVoterProgress getProgress => getProgress.VoterId,
            _ => null
        };
    }

    private new static object ExtractEntityMessage(object message) => message;
}
