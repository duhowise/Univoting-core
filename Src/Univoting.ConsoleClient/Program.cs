using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Univoting.Services;

namespace Univoting.ConsoleClient
{
    class Program
    {
        private static IConfiguration _configuration;

        public Program(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private static LiveViewService.LiveViewServiceClient _liveViewService;
        private static voteCountResult _positionCount=null;
        private static voteCountResult _positionSkippedCount=null;

        protected static Univoting.Services.LiveViewService.LiveViewServiceClient LiveViewServiceClient
        {
            get
            {
                if (_liveViewService != null) return _liveViewService;
                var channel = GrpcChannel.ForAddress(_configuration.GetValue<string>("ServerAddress"));
                _liveViewService = new Univoting.Services.LiveViewService.LiveViewServiceClient(channel);
                return _liveViewService;
            }

        }
        static async Task Main(string[] args)
        {
            var positionsResult =await LiveViewServiceClient.GetAllPositionsAsync(new GetAllPositionsRequest
            {
                ElectionId = ""
            });



            foreach (var position in positionsResult.Positions)
            {
                _positionCount = await LiveViewServiceClient.GetVotesForPositionAsync(new voteCountRequest
                {
                    PositionId = position.PositionId
                });
                
                _positionSkippedCount = await LiveViewServiceClient.GetSkippedVoteForPositionAsync(new voteCountRequest
                {
                    PositionId = position.PositionId
                });


                Console.WriteLine($"Position name {position.PositionName} has {_positionCount.Count} votes and {_positionSkippedCount.Count} Skipped votes");
            }
        }
    }
}
