namespace Domain.Enums
{
    /// <summary>Pé preferido de um jogador.</summary>
    public enum PreferredFoot
    {
        RIGHT = 0,
        LEFT = 1,
        BOTH = 2
    }

    /// <summary>Situação atual do jogador (mostrada no perfil).</summary>
    public enum PlayerStatus
    {
        /// <summary>Disponível para jogar.</summary>
        ACTIVE = 0,

        /// <summary>Lesionado: não entra no preenchimento automático do onze.</summary>
        INJURED = 1,

        /// <summary>Indisponível por outro motivo: também não entra no preenchimento automático.</summary>
        UNAVAILABLE = 2
    }

    /// <summary>Fases de uma época de uma liga.</summary>
    public enum SeasonStatus
    {
        /// <summary>Aberta a inscrições; ainda sem calendário.</summary>
        REGISTRATION = 0,

        /// <summary>Calendário sorteado e jogos a decorrer.</summary>
        IN_PROGRESS = 1,

        /// <summary>Terminada: troféu atribuído, subidas e descidas aplicadas.</summary>
        FINISHED = 2
    }

    /// <summary>Estados de uma proposta de transferência.</summary>
    public enum TransferOfferStatus
    {
        /// <summary>À espera do administrador da equipa atual do jogador.</summary>
        PENDING_CLUB = 0,

        /// <summary>O clube já aceitou; falta o jogador.</summary>
        PENDING_PLAYER = 1,

        /// <summary>Concluída: o jogador mudou de equipa.</summary>
        ACCEPTED = 2,

        /// <summary>Recusada pelo clube ou pelo jogador.</summary>
        REJECTED = 3,

        /// <summary>Retirada pela equipa que a fez, ou anulada porque o jogador mudou de equipa.</summary>
        CANCELLED = 4
    }

    /// <summary>Tipo de cartão.</summary>
    public enum CardType
    {
        YELLOW = 0,
        RED = 1
    }

    /// <summary>Tipo de evento registado no fim de um jogo.</summary>
    public enum MatchEventType
    {
        /// <summary>Golo: jogador = marcador, relacionado = assistência.</summary>
        GOAL = 0,

        YELLOW_CARD = 1,

        RED_CARD = 2,

        /// <summary>Substituição: jogador = sai, relacionado = entra.</summary>
        SUBSTITUTION = 3
    }

    /// <summary>Tipo de movimento no histórico de transferências de um jogador.</summary>
    public enum TransferKind
    {
        /// <summary>Mudou de uma equipa para outra.</summary>
        TRANSFER = 0,

        /// <summary>Entrou numa equipa sem vir de outra (pedido de adesão ou convite, ou criou a equipa).</summary>
        JOINED = 1,

        /// <summary>Saiu da equipa e ficou livre.</summary>
        LEFT = 2
    }
}
