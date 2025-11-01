namespace Domain.Constants
{
    // Uma classe estática para guardar todas as constantes
    public static class ModelConstants
    {
        public static class GeneralConst
        {
            public const int MinGoals = 0;
            public const int MaxGoals = 100;
            public const int MinAddressLength = 5;
            public const int MaxAddressLength = 250;
            public const string DefaultRankName = "Unranked";

        }
        public static class TeamConst
        {
            public const int MaxNameLength = 50;
            public const int MinNameLength = 3;
            public const int MaxDescriptionLength = 250;
            public const int MaxAdmins = 4;
            public const int MinAdmins = 1;
            public const int MinMembers = 1;
            public const int MaxMembers = 32;
            public const int MinNumberPoints = 0;
            public const int MaxNumberPoints = int.MaxValue;
            public const float MinAverageAge = UserConst.MinAge;
            public const float MaxAverageAge = UserConst.MaxAge;
        }

        public static class UserConst
        {
            public const int MaxNameLength = 100;
            public const int MinNameLength = 3;
            public const int MinEmailLength = 4;
            public const int MaxEmailLength = 50;
            public const int MinPasswordLength = 8;
            public const int MaxPasswordLength = 100;
            public const int SizePhoneNumber = 9;
            public const int MinAge = 18;
            public const int MaxAge = 70;
        }

        public static class PlayerConst
        {
            public const int MaxPositionLength = 12;
            public const int MinHeight = 100;
            public const int MaxHeight = 250;
        }

        public static class PitchConst
        {
            public const int MinNameLength = 3;
            public const int MaxNameLength = 50;
        }

        public static class RankConts
        {
            public const int MinNameLength = 1;
            public const int MaxNameLength = 50;
            public const int MinPointsWin = 1;
            public const int MaxPointsWin = int.MaxValue;
            public const int MinPointLose = int.MinValue;
            public const int MaxPointLose = 0;
            public const int MinPointToPromotion = 0;
            public const int MaxPointToPromotion = int.MaxValue;
        }

        public static class MessageConst
        {
            public const int MinMessageLength = 1;
            public const int MaxMessageLength = 250;
        }

        public static class CancelledMatchConst 
        {
            public const int MinDescriptionLength = 1;
            public const int MaxDescriptionLength = 50;
        }

        public static class StartMatchHubConst 
        {
            public const string PrefixGroupName = "StartMatchhub-";
            public const string PrefixHubCache = "StartMatchhub-";
        }
       
        public static class FinishMatchHubConst
        {
            public const string ContentMatchId = "HubMatchId";
            public const string ContentTeamId = "HubTeamId";
            public const string PrefixHubCache = "hubFinishMatch-";
            public const string PrefixGroupName = "hubFinishMatch-";
        }

        public static class RankMatchMakerHubConst {
            public const string ContentTeamId = "HubTeamId";
            public const string PrefixGroupName = "RankMatchhub-";
        }

        public static class ManagerRankMatchMakerServiceConst {
            public const string PrefixHubCache = "matchRankMaker-";
            public const string GlobalHubKeysCacheKey = "RankMatchMaker:Keys";
        }


        public static class GeralTimeInHubConst
        {
            public const int timeInMatchMackerHub = 30; //minutos
        }

        public static class DeafultCriteriaMatchMaker
        {
            public const float differenceAverageAge = 3f;
            public const float maxDifferenceAverageAge = 4.5f;
            public const int differencePoint = 9;
            public const int maxDifferencePoint = 20;
        }

        public static class HoursValidToCompetitiveMatch 
        {
            public static readonly TimeOnly MORNING = new TimeOnly(10, 0, 0);
            public static readonly TimeOnly AFTERNOON = new TimeOnly(16, 0, 0);
            public static readonly TimeOnly NIGHT = new TimeOnly(19, 0, 0);
        }
    }
}

