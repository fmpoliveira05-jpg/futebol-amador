using Domain.Entities;

namespace Application.Services.Competition
{
    /// <summary>Regras do administrador principal (ver docs/novas-funcionalidades.md, D9).</summary>
    public static class TeamHierarchy
    {
        /// <summary>
        /// Quem passa a administrador principal quando <paramref name="leavingPlayerId"/> sai: o administrador
        /// há mais tempo; sem outros administradores, nulo (quem chama decide se promove alguém).
        /// </summary>
        public static Player? NextSupremeAdmin(Team team, string leavingPlayerId) =>
            team.Members
                .Where(m => m.Id != leavingPlayerId && m.IsAdmin)
                .OrderBy(m => m.IsAdminLastChangedAt ?? DateTime.MaxValue)
                .ThenBy(m => m.Id, StringComparer.Ordinal)
                .FirstOrDefault();

        /// <summary>
        /// Id do administrador principal. Nas equipas antigas sem <see cref="Team.CreatorId"/>, é o administrador
        /// há mais tempo (a mesma regra usada na migração).
        /// </summary>
        public static string? EffectiveCreatorId(Team team) =>
            team.CreatorId
            ?? team.Members
                .Where(m => m.IsAdmin)
                .OrderBy(m => m.IsAdminLastChangedAt ?? DateTime.MaxValue)
                .ThenBy(m => m.Id, StringComparer.Ordinal)
                .FirstOrDefault()?.Id;

        /// <summary>O jogador é o administrador principal da equipa.</summary>
        public static bool IsSupreme(Team team, string? playerId) =>
            playerId != null && EffectiveCreatorId(team) == playerId;
    }
}
