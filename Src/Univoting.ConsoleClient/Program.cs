﻿using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Univoting.Services;

namespace Univoting.ConsoleClient
{
    class Program
    {
        private static IConfiguration _configuration;

        public Program()
        {
            _configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .Build();


        }
        private static LiveViewService.LiveViewServiceClient _liveViewService;
        private static voteCountResult _positionCount=null;
        private static voteCountResult _positionSkippedCount=null;

        protected static Univoting.Services.LiveViewService.LiveViewServiceClient LiveViewServiceClient
        {
            get
            {
                if (_liveViewService != null) return _liveViewService;
                var channel = GrpcChannel.ForAddress("https://localhost:44348");
                _liveViewService = new Univoting.Services.LiveViewService.LiveViewServiceClient(channel);
                return _liveViewService;
            }

        }
        static async Task Main(string[] args)
        {
            var positionsResult =await LiveViewServiceClient.GetAllPositionsAsync(new GetAllPositionsRequest
            {
                ElectionId = "3f8e6896-4c7d-15f5-a018-75d8bd200d7c"
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