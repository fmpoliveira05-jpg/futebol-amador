namespace Domain.Constants
{
    /// <summary>
    /// Classe estática que agrega todas as constantes e limites de validação (Constraints)
    /// utilizados pelas entidades de domínio e pela lógica de negócio.
    /// </summary>
    public static class ModelConstants
    {
        /// <summary>
        /// Constantes gerais aplicáveis a várias entidades (Golos, Morada, Cidades, Ranks).
        /// </summary>
        public static class GeneralConst
        {
            /// <summary> O número mínimo de golos em uma partida. </summary>
            public const int MinGoals = 0;
            /// <summary> O número máximo de golos permitido em uma partida. </summary>
            public const int MaxGoals = 100;
            /// <summary> Comprimento mínimo para campos de morada. </summary>
            public const int MinAddressLength = 5;
            /// <summary> Comprimento máximo para campos de morada. </summary>
            public const int MaxAddressLength = 250;
            /// <summary> Comprimento mínimo para nomes de cidade. </summary>
            public const int MinCityLength = 1;
            /// <summary> Comprimento máximo para nomes de cidade. </summary>
            public const int MaxCityLength = 50;
            /// <summary> Nome padrão para o rank inicial ou sem classificação. </summary>
            public const string DefaultRankName = "Unranked";
        }

        /// <summary>
        /// Constantes e limites de validação específicos para a entidade Equipa ([Team]).
        /// </summary>
        public static class TeamConst
        {
            /// <summary> Comprimento máximo para o nome da equipa. </summary>
            public const int MaxNameLength = 50;
            /// <summary> Comprimento mínimo para o nome da equipa. </summary>
            public const int MinNameLength = 3;
            /// <summary> Comprimento máximo para a descrição da equipa. </summary>
            public const int MaxDescriptionLength = 250;
            /// <summary> Número máximo de administradores permitidos por equipa. </summary>
            public const int MaxAdmins = 4;
            /// <summary> Número mínimo de administradores exigidos por equipa. </summary>
            public const int MinAdmins = 1;
            /// <summary> Número mínimo de membros (jogadores/staff) em uma equipa. </summary>
            public const int MinMembers = 1;

            public const int MimMembersToMatch = 11;
            /// <summary> Número máximo de membros em uma equipa. </summary>
            public const int MaxMembers = 32;
            /// <summary> Valor mínimo para os pontos de ranking. </summary>
            public const int MinNumberPoints = 0;
            /// <summary> Valor máximo para os pontos de ranking. </summary>
            public const int MaxNumberPoints = int.MaxValue;
            /// <summary> Idade média mínima permitida para uma equipa (igual à idade mínima do utilizador). </summary>
            public const float MinAverageAge = UserConst.MinAge;
            /// <summary> Idade média máxima permitida para uma equipa (igual à idade máxima do utilizador). </summary>
            public const float MaxAverageAge = UserConst.MaxAge;
        }

        /// <summary>
        /// Constantes e limites de validação específicos para a entidade Utilizador ([User]).
        /// </summary>
        public static class UserConst
        {
            /// <summary> Comprimento máximo para o nome de utilizador. </summary>
            public const int MaxNameLength = 100;
            /// <summary> Comprimento mínimo para o nome de utilizador. </summary>
            public const int MinNameLength = 3;
            /// <summary> Comprimento mínimo para endereços de e-mail. </summary>
            public const int MinEmailLength = 4;
            /// <summary> Comprimento máximo para endereços de e-mail. </summary>
            public const int MaxEmailLength = 256;
            /// <summary> Comprimento fixo esperado para números de telefone (incluindo código de país). </summary>
            public const int SizePhoneNumber = 13;
            /// <summary> Idade mínima permitida para o registo de utilizador. </summary>
            public const int MinAge = 18;
            /// <summary> Idade máxima permitida para o registo de utilizador. </summary>
            public const int MaxAge = 70;
            /// <summary> Comprimento máximo para o ID do utilizador (Firebase UID). </summary>
            public const int MaxIdLength = 128;
        }
        
        /// <summary>
        /// Constantes e limites de validação específicos para a entidade Jogador ([Player]).
        /// </summary>
        public static class PlayerConst
        {
            /// <summary> Comprimento máximo para o nome da posição (embora se use enum, pode ser usado em outros DTOs). </summary>
            public const int MaxPositionLength = 12;
            /// <summary> Altura mínima permitida para um jogador (em cm). </summary>
            public const int MinHeight = 100;
            /// <summary> Altura máxima permitida para um jogador (em cm). </summary>
            public const int MaxHeight = 250;
        }

        /// <summary>
        /// Constantes e limites de validação específicos para a entidade Campo ([Pitch]).
        /// </summary>
        public static class PitchConst
        {
            /// <summary> Comprimento mínimo para o nome do campo. </summary>
            public const int MinNameLength = 3;
            /// <summary> Comprimento máximo para o nome do campo. </summary>
            public const int MaxNameLength = 50;
        }

        /// <summary>
        /// Constantes e limites de validação específicos para a entidade Rank ([Rank]).
        /// </summary>
        public static class RankConts
        {
            /// <summary> Comprimento mínimo para o nome do Rank. </summary>
            public const int MinNameLength = 1;
            /// <summary> Comprimento máximo para o nome do Rank. </summary>
            public const int MaxNameLength = 50;
            /// <summary> Pontos mínimos que devem ser ganhos por vitória. </summary>
            public const int MinPointsWin = 1;
            /// <summary> Pontos máximos que podem ser ganhos por vitória. </summary>
            public const int MaxPointsWin = int.MaxValue;
            /// <summary> Pontos mínimos (negativos) que podem ser perdidos por derrota. </summary>
            public const int MinPointLose = int.MinValue;
            /// <summary> Pontos máximos (zero) que podem ser perdidos por derrota. </summary>
            public const int MaxPointLose = 0;
            /// <summary> Pontos mínimos necessários para atingir este rank. </summary>
            public const int MinPointToPromotion = 0;
            /// <summary> Pontos máximos necessários para atingir este rank. </summary>
            public const int MaxPointToPromotion = int.MaxValue;
        }

        /// <summary>
        /// Constantes e limites de validação específicos para a entidade Mensagem ([Message]).
        /// </summary>
        public static class MessageConst
        {
            /// <summary> Comprimento mínimo para o conteúdo de uma mensagem. </summary>
            public const int MinMessageLength = 1;
            /// <summary> Comprimento máximo para o conteúdo de uma mensagem. </summary>
            public const int MaxMessageLength = 250;
        }

        /// <summary>
        /// Constantes e limites de validação específicos para a entidade Jogo Cancelado ([CancelledMatch]).
        /// </summary>
        public static class CancelledMatchConst
        {
            /// <summary> Comprimento mínimo para o campo de descrição do cancelamento. </summary>
            public const int MinDescriptionLength = 1;
            /// <summary> Comprimento máximo para o campo de descrição do cancelamento. </summary>
            public const int MaxDescriptionLength = 50;
        }

        /// <summary>
        /// Constantes de configuração para o Hub SignalR de Início de Partida (Start Match Hub).
        /// </summary>
        public static class StartMatchHubConst
        {
            /// <summary> Prefixo para o nome do grupo de utilizadores a subscrever no Hub. </summary>
            public const string PrefixGroupName = "StartMatchhub-";
            /// <summary> Prefixo para chaves de cache relacionadas com o Hub. </summary>
            public const string PrefixHubCache = "StartMatchhub-";
        }

        /// <summary>
        /// Constantes de configuração para o Hub SignalR de Finalização de Partida (Finish Match Hub).
        /// </summary>
        public static class FinishMatchHubConst
        {
            /// <summary> Chave para o ID da Partida em mensagens do Hub. </summary>
            public const string ContentMatchId = "HubMatchId";
            /// <summary> Chave para o ID da Equipa em mensagens do Hub. </summary>
            public const string ContentTeamId = "HubTeamId";
            /// <summary> Prefixo para chaves de cache relacionadas com o Hub. </summary>
            public const string PrefixHubCache = "hubFinishMatch-";
            /// <summary> Prefixo para o nome do grupo de utilizadores a subscrever no Hub. </summary>
            public const string PrefixGroupName = "hubFinishMatch-";
        }

        /// <summary>
        /// Constantes de configuração para o Hub SignalR de Matchmaker de Partidas Ranqueadas.
        /// </summary>
        public static class RankMatchMakerHubConst
        {
            /// <summary> Chave para o ID da Equipa em mensagens do Hub. </summary>
            public const string ContentTeamId = "HubTeamId";
            /// <summary> Prefixo para o nome do grupo de utilizadores a subscrever no Hub. </summary>
            public const string PrefixGroupName = "RankMatchhub-";
        }

        /// <summary>
        /// Constantes de configuração para o Serviço de Gestão do Matchmaker de Partidas Ranqueadas.
        /// </summary>
        public static class ManagerRankMatchMakerServiceConst
        {
            /// <summary> Prefixo para chaves de cache relacionadas com o Matchmaker. </summary>
            public const string PrefixHubCache = "matchRankMaker-";
            /// <summary> Chave de cache global que lista todas as chaves de Hubs ativos. </summary>
            public const string GlobalHubKeysCacheKey = "RankMatchMaker:Keys";
        }

        /// <summary>
        /// Constantes de configuração para o Hub SignalR de Notificações.
        /// </summary>
        public static class NotificationHubConst
        {
            /// <summary> Prefixo para o nome do grupo de utilizadores a subscrever no Hub. </summary>
            public const string PrefixGroupName = "notificationGroup-";
            /// <summary> Chave para o ID da Equipa em mensagens do Hub. </summary>
            public const string ContentTeamId = "HubTeamId";
        }

        /// <summary>
        /// Constantes de tempo de vida de sessões ou objetos em Hubs.
        /// </summary>
        public static class GeralTimeInHubConst
        {
            /// <summary> Tempo máximo em minutos que um utilizador pode esperar no lobby de Matchmaker. </summary>
            public const int timeInMatchMackerHub = 30; //minutos
        }

        /// <summary>
        /// Critérios padrão de tolerância para o algoritmo de Matchmaker de partidas ranqueadas.
        /// </summary>
        public static class DeafultCriteriaMatchMaker
        {
            /// <summary> Diferença máxima aceitável na idade média das equipas (inicial). </summary>
            public const float differenceAverageAge = 3f;
            /// <summary> Diferença máxima absoluta na idade média das equipas (tolerância máxima). </summary>
            public const float maxDifferenceAverageAge = 4.5f;
            /// <summary> Diferença máxima aceitável nos pontos de ranking das equipas (inicial). </summary>
            public const int differencePoint = 9;
            /// <summary> Diferença máxima absoluta nos pontos de ranking das equipas (tolerância máxima). </summary>
            public const int maxDifferencePoint = 20;
        }

        /// <summary>
        /// Constantes que definem os intervalos de horário válidos para agendar partidas competitivas.
        /// </summary>
        public static class HoursValidToCompetitiveMatch
        {
            /// <summary> Início do horário da manhã (10:00). </summary>
            public static readonly TimeOnly MORNING = new TimeOnly(10, 0, 0);
            /// <summary> Início do horário da tarde (16:00). </summary>
            public static readonly TimeOnly AFTERNOON = new TimeOnly(16, 0, 0);
            /// <summary> Início do horário da noite (19:00). </summary>
            public static readonly TimeOnly NIGHT = new TimeOnly(19, 0, 0);
        }

        /// <summary>
        /// Constantes relacionadas com a Tabela de Classificação (Leaderboard).
        /// </summary>
        public static class TeamLeaderBoardConst
        {
            /// <summary> A posição mais alta (1º lugar). </summary>
            public const int FirstPosition = 1;
            /// <summary> O limite de equipas exibido na tabela de classificação. </summary>
            public const int LastPosition = 100;
        }

        /// <summary>
        /// Constantes de configuração de rotas e endereços para Hubs SignalR.
        /// </summary>
        public static class RouteHubConst
        {
            /// <summary> Endereço base do servidor Hub SignalR. </summary>
            public const string StartRoute = "http://localhost:5218";
        }
    }
}

