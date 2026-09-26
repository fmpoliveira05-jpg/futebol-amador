using Domain.Enums;

namespace Application.Competition
{
    /// <summary>Uma posição de uma tática, com as coordenadas para desenhar o campo.</summary>
    /// <param name="Slot">Índice da posição (0 = guarda-redes).</param>
    /// <param name="PositionCode">Código português da posição (GR, DD, DC, DE, MDC, MC, MD, ME, MOC, ED, EE, PL).</param>
    /// <param name="Role">Posição de jogador que encaixa aqui (filtra a lista de escolha).</param>
    /// <param name="X">Percentagem da largura do campo (0 = esquerda, vista de trás da baliza da equipa).</param>
    /// <param name="Y">Percentagem do comprimento (100 = baliza da própria equipa).</param>
    public sealed record FormationSlot(int Slot, string PositionCode, Position Role, int X, int Y);

    /// <summary>Tática disponível para o onze inicial.</summary>
    public sealed record Formation(string Code, IReadOnlyList<FormationSlot> Slots);

    /// <summary>Catálogo das táticas aceites (ver ADR D7).</summary>
    public static class Formations
    {
        public const string Default = "4-3-3";

        private static readonly (string Code, (string Pos, int X, int Y)[] Outfield)[] Definitions =
        {
            ("4-4-2", new[] { ("DD", 85, 72), ("DC", 62, 76), ("DC", 38, 76), ("DE", 15, 72),
                              ("MD", 85, 48), ("MC", 62, 52), ("MC", 38, 52), ("ME", 15, 48),
                              ("PL", 62, 22), ("PL", 38, 22) }),
            ("4-3-3", new[] { ("DD", 85, 72), ("DC", 62, 76), ("DC", 38, 76), ("DE", 15, 72),
                              ("MC", 72, 50), ("MDC", 50, 56), ("MC", 28, 50),
                              ("ED", 82, 24), ("PL", 50, 18), ("EE", 18, 24) }),
            ("4-2-3-1", new[] { ("DD", 85, 72), ("DC", 62, 76), ("DC", 38, 76), ("DE", 15, 72),
                                ("MDC", 62, 58), ("MDC", 38, 58),
                                ("ED", 82, 36), ("MOC", 50, 38), ("EE", 18, 36),
                                ("PL", 50, 16) }),
            ("3-5-2", new[] { ("DC", 72, 76), ("DC", 50, 78), ("DC", 28, 76),
                              ("MD", 90, 48), ("MC", 68, 52), ("MDC", 50, 58), ("MC", 32, 52), ("ME", 10, 48),
                              ("PL", 62, 22), ("PL", 38, 22) }),
            ("3-4-3", new[] { ("DC", 72, 76), ("DC", 50, 78), ("DC", 28, 76),
                              ("MD", 85, 50), ("MC", 62, 54), ("MC", 38, 54), ("ME", 15, 50),
                              ("ED", 80, 24), ("PL", 50, 18), ("EE", 20, 24) }),
            ("5-3-2", new[] { ("DD", 90, 66), ("DC", 70, 76), ("DC", 50, 78), ("DC", 30, 76), ("DE", 10, 66),
                              ("MC", 72, 50), ("MC", 50, 54), ("MC", 28, 50),
                              ("PL", 62, 22), ("PL", 38, 22) }),
            ("4-5-1", new[] { ("DD", 85, 72), ("DC", 62, 76), ("DC", 38, 76), ("DE", 15, 72),
                              ("MD", 88, 46), ("MC", 68, 52), ("MDC", 50, 58), ("MC", 32, 52), ("ME", 12, 46),
                              ("PL", 50, 18) }),
        };

        /// <summary>Todas as táticas, pela ordem em que aparecem na interface.</summary>
        public static IReadOnlyList<Formation> All { get; } = Definitions
            .Select(d => new Formation(d.Code, new[] { new FormationSlot(0, "GR", Position.GOALKEEPER, 50, 92) }
                .Concat(d.Outfield.Select((o, i) => new FormationSlot(i + 1, o.Pos, RoleOf(o.Pos), o.X, o.Y)))
                .ToList()))
            .ToList();

        public static Formation? Find(string? code) =>
            All.FirstOrDefault(f => string.Equals(f.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase));

        /// <summary>Posição de jogador que corresponde a um código de posição da tática.</summary>
        public static Position RoleOf(string positionCode) => positionCode switch
        {
            "GR" => Position.GOALKEEPER,
            "DD" or "DC" or "DE" => Position.DEFENDER,
            "MDC" or "MC" or "MD" or "ME" or "MOC" => Position.MIDFIELDER,
            _ => Position.FORWARD,
        };
    }
}
