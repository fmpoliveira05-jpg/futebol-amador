using Application.DTOs.Competition;
using Application.Services.Competition;
using Domain.Enums;
using Domain.Exceptions;
using NUnit.Framework;

namespace Tests.Unit.CompetitionTests
{
    [TestFixture]
    public class MatchEventsValidationTests
    {
        private static readonly HashSet<string> Squad = new() { "a", "b", "c", "s1", "s2" };
        private static readonly HashSet<string> Starters = new() { "a", "b", "c" };

        [Test]
        public void Valid_EventsPass()
        {
            var events = new MatchEventsDto
            {
                Fouls = 12,
                Goals = { new GoalEventDto { ScorerId = "a", AssistId = "b", Minute = 10 }, new GoalEventDto { Minute = 50 } },
                Cards = { new CardEventDto { PlayerId = "c", Type = CardType.YELLOW, Minute = 30 } },
                Substitutions = { new SubstitutionEventDto { PlayerOutId = "a", PlayerInId = "s1", Minute = 60 } },
            };

            Assert.DoesNotThrow(() => MatchDetailsService.ValidateEvents(events, 2, Squad, Starters));
        }

        [Test(Description = "Não se registam mais golos do que os da equipa.")]
        public void MoreGoalsThanScore_Throws()
        {
            var events = new MatchEventsDto { Goals = { new GoalEventDto { ScorerId = "a" }, new GoalEventDto { ScorerId = "b" } } };
            Assert.Throws<ValidationException>(() => MatchDetailsService.ValidateEvents(events, 1, Squad, Starters));
        }

        [Test]
        public void PlayerFromOutsideTheTeam_Throws()
        {
            var events = new MatchEventsDto { Cards = { new CardEventDto { PlayerId = "intruso", Type = CardType.RED } } };
            Assert.Throws<ValidationException>(() => MatchDetailsService.ValidateEvents(events, 0, Squad, Starters));
        }

        [Test]
        public void OwnAssist_Throws()
        {
            var events = new MatchEventsDto { Goals = { new GoalEventDto { ScorerId = "a", AssistId = "a" } } };
            Assert.Throws<ValidationException>(() => MatchDetailsService.ValidateEvents(events, 1, Squad, Starters));
        }

        [Test(Description = "Quem entra tem de vir do banco, e só entra uma vez.")]
        public void SubstitutionFromStarterOrTwice_Throws()
        {
            var fromStarter = new MatchEventsDto { Substitutions = { new SubstitutionEventDto { PlayerOutId = "a", PlayerInId = "b" } } };
            var twice = new MatchEventsDto
            {
                Substitutions =
                {
                    new SubstitutionEventDto { PlayerOutId = "a", PlayerInId = "s1", Minute = 50 },
                    new SubstitutionEventDto { PlayerOutId = "b", PlayerInId = "s1", Minute = 70 },
                },
            };

            Assert.Throws<ValidationException>(() => MatchDetailsService.ValidateEvents(fromStarter, 0, Squad, Starters));
            Assert.Throws<ValidationException>(() => MatchDetailsService.ValidateEvents(twice, 0, Squad, Starters));
        }

        [Test]
        public void TwoRedCardsForSamePlayer_Throws()
        {
            var events = new MatchEventsDto
            {
                Cards = { new CardEventDto { PlayerId = "a", Type = CardType.RED }, new CardEventDto { PlayerId = "a", Type = CardType.RED } },
            };
            Assert.Throws<ValidationException>(() => MatchDetailsService.ValidateEvents(events, 0, Squad, Starters));
        }
    }
}
