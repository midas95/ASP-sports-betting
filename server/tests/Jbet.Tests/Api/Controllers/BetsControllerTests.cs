﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using Jbet.Api.Hateoas;
using Jbet.Api.Hateoas.Resources.Bet;
using Jbet.Core.UserMatchBetContext.HttpRequests;
using Jbet.Domain.Entities;
using Jbet.Tests.Business.AuthContext;
using Jbet.Tests.Business.TeamContext.Helpers;
using Jbet.Tests.Customizations;
using Jbet.Tests.Extensions;
using Xunit;

namespace Jbet.Tests.Api.Controllers
{
    public class BetsControllerTests : ResetDatabaseLifetime
    {
        private readonly AppFixture _fixture;
        private readonly ApiTestsHelper _apiHelper;
        private readonly TeamTestsHelper _teamTestsHelper;

        public BetsControllerTests()
        {
            _fixture = new AppFixture();
            _apiHelper = new ApiTestsHelper(_fixture);
            _teamTestsHelper = new TeamTestsHelper(_fixture);
        }

        [Theory]
        [CustomizedAutoData]
        public async Task BetForAwayTeamShouldReturnProperHypermediaLinks(
            Fixture fixture,
            Team homeTeam,
            Team awayTeam) =>
            await _apiHelper.InTheContextOfAnAuthenticatedUser(
                async client =>
                {
                    // Arrange
                    var currentMatch = await RegistrateMatchAsync(fixture, homeTeam, awayTeam);

                    var httpRequest = new MatchAwayBetInput
                    {
                        MatchId = currentMatch.Id,
                        AwayBet = fixture.Create<decimal>(),
                    };

                    // Act
                    var response = await client.PostAsJsonAsync("/bets/away-team", httpRequest);

                    // Assert
                    var expectedLinks = new List<string>
                    {
                        LinkNames.Self
                    };

                    await response.ShouldBeAResource<UserMatchBetResource>(expectedLinks);

                }, fixture);

        [Theory]
        [CustomizedAutoData]
        public async Task BetForHomeTeamShouldReturnProperHypermediaLinks(
            Fixture fixture,
            Team homeTeam,
            Team awayTeam) =>
            await _apiHelper.InTheContextOfAnAuthenticatedUser(
                async client =>
                {
                    // Arrange
                    var currentMatch = await RegistrateMatchAsync(fixture, homeTeam, awayTeam);

                    var httpRequest = new MatchHomeBetInput
                    {
                        MatchId = currentMatch.Id,
                        HomeBet = fixture.Create<decimal>(),
                    };

                    // Act
                    var response = await client.PostAsJsonAsync("/bets/home-team", httpRequest);

                    // Assert
                    var expectedLinks = new List<string>
                    {
                        LinkNames.Self
                    };

                    await response.ShouldBeAResource<UserMatchBetResource>(expectedLinks);

                }, fixture);

        private async Task<Match> RegistrateMatchAsync(
            ISpecimenBuilder fixture,
            Team homeTeam,
            Team awayTeam)
        {
            var team1 = await _teamTestsHelper.AddAsync(homeTeam);
            var team2 = await _teamTestsHelper.AddAsync(awayTeam);

            var currentMatch = new Match
            {
                Id = Guid.NewGuid(),
                AwayTeamId = team2.Id,
                HomeTeamId = team1.Id,
                Start = fixture.Create<DateTime>(),
            };

            await _fixture.ExecuteDbContextAsync(async dbContext =>
            {
                await dbContext.Matches.AddAsync(currentMatch);
                await dbContext.SaveChangesAsync();
            });

            return currentMatch;
        }
    }
}