using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Univoting.Services.Implementations
{
    public class LiveViewService:Services.LiveViewService.LiveViewServiceBase
    {
        public override Task<voteCountResult> GetVotesForPosition(voteCountRequest request, ServerCallContext context)
        {
            var positionId = request.PositionId;
           return Task.FromResult(new voteCountResult
           {
               Count = Convert.ToInt64(40)
           });
        }

        public override Task<AllPositionsResult> GetAllPositions(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new AllPositionsResult
            {
                Positions = {new Position{ElectionId = "44F8DF3F-6D4F-4F41-AFD4-856910DC23F8", PositionId = "",PositionName = ""}, new Position { ElectionId = "44F8DF3F-6D4F-4F41-AFD4-856910DC23F8", PositionId = "", PositionName = "" } }
            });
        }

        public override Task<voteCountResult> GetSkippedVoteForPosition(voteCountRequest request, ServerCallContext context)
        {
            return Task.FromResult(new voteCountResult
            {
                Count = Convert.ToInt64(40)
            });
        }
    }
}
