using Application.Services.Competition;
using Application.Validators;
using Domain.Entities;
using Domain.Exceptions;
using NUnit.Framework;

namespace Tests.Unit.CompetitionTests
{
    /// <summary>Administrador principal: só ele despromove e remove administradores, e ninguém o despromove.</summary>
    [TestFixture]
    public class TeamHierarchyTests
    {
        private Team team = null!;
        private Player creator = null!, admin = null!, member = null!;
        private readonly TeamValidator validator = new();

        [SetUp]
        public void SetUp()
        {
            team = new Team { Id = Guid.NewGuid(), Name = "Equipa" };
            creator = new Player { Id = "criador", IsAdmin = true, IsAdminLastChangedAt = new DateTime(2025, 1, 1), Team = team };
            admin = new Player { Id = "admin", IsAdmin = true, IsAdminLastChangedAt = new DateTime(2025, 6, 1), Team = team };
            member = new Player { Id = "membro", Team = team };
            team.Members = new List<Player> { creator, admin, member };
            team.CreatorId = creator.Id;
        }

        [Test]
        public void CreatorCanDemoteAdmin() =>
            Assert.DoesNotThrow(() => validator.DemoteAdminToMemberValidation(team, admin, creator));

        [Test]
        public void OtherAdminCannotDemote()
        {
            var another = new Player { Id = "outro", IsAdmin = true, Team = team };
            team.Members.Add(another);
            Assert.Throws<ForbiddenException>(() => validator.DemoteAdminToMemberValidation(team, another, admin));
        }

        [Test]
        public void NobodyDemotesTheCreator() =>
            Assert.Throws<ValidationException>(() => validator.DemoteAdminToMemberValidation(team, creator, admin));

        [Test]
        public void OnlyCreatorRemovesAdmins()
        {
            Assert.Throws<ForbiddenException>(() => validator.RemovePlayerFromTeamValidation(team, admin, new Player { Id = "x", IsAdmin = true, Team = team }));
            Assert.DoesNotThrow(() => validator.RemovePlayerFromTeamValidation(team, creator, admin));
            Assert.DoesNotThrow(() => validator.RemovePlayerFromTeamValidation(team, admin, member));
            Assert.Throws<ValidationException>(() => validator.RemovePlayerFromTeamValidation(team, admin, creator));
        }

        [Test(Description = "Equipas antigas sem criador registado: conta o administrador mais antigo.")]
        public void LegacyTeam_OldestAdminIsCreator()
        {
            team.CreatorId = null;
            Assert.That(TeamHierarchy.EffectiveCreatorId(team), Is.EqualTo(creator.Id));
            Assert.DoesNotThrow(() => validator.DemoteAdminToMemberValidation(team, admin, creator));
        }

        [Test]
        public void NextSupremeAdmin_IsOldestRemainingAdmin()
        {
            Assert.That(TeamHierarchy.NextSupremeAdmin(team, creator.Id)?.Id, Is.EqualTo(admin.Id));
            admin.IsAdmin = false;
            Assert.That(TeamHierarchy.NextSupremeAdmin(team, creator.Id), Is.Null);
        }
    }
}
