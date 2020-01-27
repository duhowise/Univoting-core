using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Univoting.Data;

namespace Univoting.Services.Implementations
{
    public class LiveViewService:Services.LiveViewService.LiveViewServiceBase
    {
        private readonly UnivotingContext _context;

        public LiveViewService(UnivotingContext context)
        {
            _context = context;
        }
        public override async Task<voteCountResult> GetVotesForPosition(voteCountRequest request, ServerCallContext context)
        {
            Guid.TryParse(request.PositionId, out var positionId);
            return new voteCountResult
            {
                Count = await _context.Votes.Where(x => x.PositionId == positionId).CountAsync()
            };
        }

        public override async Task<AllPositionsResult> GetAllPositions(GetAllPositionsRequest request, ServerCallContext context)
        {
            try
            {
                Guid.TryParse(request.ElectionId, out var electionId);
                var positions = await _context.Positions.Where(x => x.ElectionId == electionId).Select(x=> new Position
                {
                    PositionId = x.Id.ToString(),ElectionId = x.ElectionId.ToString(),PositionName = x.Name
                }).ToListAsync();

                var result=new AllPositionsResult();
                result.Positions.AddRange(positions);

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public override async Task<voteCountResult> GetSkippedVoteForPosition(voteCountRequest request, ServerCallContext context)
        {
            Guid.TryParse(request.PositionId, out var positionId);
            return new voteCountResult
            {
                Count = await _context.Votes.Where(x => x.PositionId == positionId).CountAsync()
            };
        }
    }
}
