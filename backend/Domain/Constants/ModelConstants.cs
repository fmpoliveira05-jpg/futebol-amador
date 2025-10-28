namespace Domain.Constants
{
    // Uma classe estática para guardar todas as constantes
    public static class ModelConstants
    {
        public static class GeneralConst
        {
            public const int MinAge = 18;
            public const int MaxAge = 70;
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
            public const int MaxPlayers = 32;
            public const int MaxAdmins = 4;
            public const int MinNumberPoints = 0;
            public const int MaxNumberPoints = int.MaxValue;
            public const float MinAverageAge = GeneralConst.MinAge;
            public const float MaxAverageAge = GeneralConst.MaxAge;
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

        public static class FinishMatchHubConst
        {
            public const string ContentMatchId = "HubMatchId";
            public const string ContentTeamId = "HubTeamId";
        }

        public static class RankMatchMakerHubConst {
            public const string ContentTeamId = "HubTeamId";
        }
    }
}

