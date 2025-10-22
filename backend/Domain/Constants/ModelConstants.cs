using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            public const int MaxAddressLength = 250;


        }
        public static class TeamConst
        {
            public const int MaxNameLength = 50;
            public const int MinNameLength = 3;
            public const int MaxDescriptionLength = 250;
            public const int MaxPlayers = 32; //Validar se é mesmo 32
            public const int MaxAdmins = 4;
        }

        public static class UserConst
        {
            public const int MaxNameLength = 100;
            public const int MinNameLength = 3;
            public const int MaxNameLengthEmail = 50;
            public const int MinPasswordLength = 8;
            public const int MaxPasswordLength = 16;
        }

        public static class PlayerConst
        {
            public const int MaxPositionLength = 12;
            public const int MinHeight = 100;
            public const int MaxHeight = 250;
        }

        public static class PitchConst
        {
            public const int MaxNameLength = 50;
        }
    }
}

