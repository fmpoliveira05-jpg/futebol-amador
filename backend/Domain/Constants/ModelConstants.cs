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
            /// <summary> Comprimento máximo do emblema quando é um URL (Cloudinary). </summary>
            public const int MaxIconUrlLength = 512;
            /// <summary> Comprimento máximo do emblema quando é uma imagem embebida (data URL de ~256x256 px). </summary>
            public const int MaxIconDataUrlLength = 200_000;
            /// <summary> Anfitriões de onde se aceitam emblemas por URL (só HTTPS). </summary>
            public static readonly string[] AllowedIconHosts = { "res.cloudinary.com" };
        }

        /// <summary>
        /// Regras das palavras-passe (registo, alteração e criação de super administradores).
        /// </summary>
        public static class PasswordConst
        {
            /// <summary> Comprimento mínimo. </summary>
            public const int MinLength = 10;
            /// <summary> Comprimento máximo (o Firebase Authentication não aceita mais de 128). </summary>
            public const int MaxLength = 128;
        }

        /// <summary>
        /// Limites das salas de chat criadas pela API.
        /// </summary>
        public static class ChatConst
        {
            /// <summary> Número máximo de membros de uma sala (inclui quem a cria). </summary>
            public const int MaxMembers = 40;
            /// <summary> Comprimento máximo do nome da sala. </summary>
            public const int MaxRoomNameLength = 80;
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

            /// <summary>Peso mínimo aceite (kg).</summary>
            public const int MinWeight = 40;

            /// <summary>Peso máximo aceite (kg).</summary>
            public const int MaxWeight = 150;

            /// <summary>Comprimento máximo do nome de um país.</summary>
            public const int MaxCountryLength = 56;
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

            /// <summary>Comprimento máximo do motivo de um adiamento ou de um cancelamento com nova data.</summary>
            public const int MaxReasonLength = 250;
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
        /// Constantes de configuração de rotas e endereços para Hubs SignalR.
        /// </summary>
        public static class RouteHubConst
        {
            /// <summary> Endereço base do servidor Hub SignalR. </summary>
            public const string StartRoute = "http://localhost:5218";
        }

        /// <summary>
        /// Constantes do onze inicial e do relatório do jogo.
        /// </summary>
        public static class LineupConst
        {
            /// <summary>Até quantas horas antes do jogo o administrador pode definir o onze.</summary>
            public const int DeadlineHours = 2;

            /// <summary>Número de titulares.</summary>
            public const int Starters = 11;

            /// <summary>Número máximo de suplentes.</summary>
            public const int MaxBench = 12;

            /// <summary>Duração assumida de um jogo, para calcular os minutos jogados.</summary>
            public const int MatchMinutes = 90;
        }

        /// <summary>
        /// Constantes das ligas e da classificação.
        /// </summary>
        public static class LeagueConst
        {
            public const int PointsWin = 3;

            public const int PointsDraw = 1;

            public const int PointsLoss = 0;

            /// <summary>Quantos jogos entram na "forma" da classificação.</summary>
            public const int FormLength = 5;

            /// <summary>Hora de início por omissão dos jogos sorteados.</summary>
            public const string DefaultKickoff = "15:00";
        }

        /// <summary>Constantes do RGPD (consentimento e eliminação de contas).</summary>
        public static class RgpdConst
        {
            public const int MaxVersaoPolitica = 20;

            /// <summary>Nome mostrado no lugar de um jogador que eliminou a conta.</summary>
            public const string NomeAnonimo = "Jogador removido";
        }
    }
}
