using Domain.Constants;
using Domain.Enums;

namespace Application.Competition
{
    /// <summary>Jogador disponível para o preenchimento automático.</summary>
    public sealed record SquadPlayer(string Id, string Name, Position Position, PlayerStatus Status);

    /// <summary>Resultado do preenchimento: titulares por posição da tática e suplentes por ordem.</summary>
    public sealed record AutoLineup(string Formation, IReadOnlyDictionary<int, string> Starters, IReadOnlyList<string> Bench);

    /// <summary>
    /// Preenche o onze de uma equipa que não o definiu a tempo (ver ADR D7): repete o último onze com os
    /// jogadores que ainda estão disponíveis e completa as posições vazias com jogadores dessa posição e,
    /// se faltarem, com quaisquer outros. Lesionados e indisponíveis ficam de fora.
    /// </summary>
    public static class LineupAutoFiller
    {
        /// <param name="squad">Plantel atual da equipa.</param>
        /// <param name="previousFormation">Tática do último onze (nula se não houver).</param>
        /// <param name="previousStarters">Titulares do último onze: posição → jogador.</param>
        /// <param name="previousBench">Suplentes do último onze, por ordem.</param>
        public static AutoLineup Fill(
            IReadOnlyList<SquadPlayer> squad,
            string? previousFormation,
            IReadOnlyDictionary<int, string>? previousStarters,
            IReadOnlyList<string>? previousBench)
        {
            var formation = Formations.Find(previousFormation) ?? Formations.Find(Formations.Default)!;
            var available = squad
                .Where(p => p.Status == PlayerStatus.ACTIVE)
                .OrderBy(p => p.Name, StringComparer.CurrentCulture)
                .ThenBy(p => p.Id, StringComparer.Ordinal)
                .ToList();
            var byId = available.ToDictionary(p => p.Id);
            var used = new HashSet<string>();
            var starters = new Dictionary<int, string>();

            // 1. O último onze, na mesma posição, com quem ainda está disponível.
            if (previousStarters != null && previousFormation != null && Formations.Find(previousFormation) != null)
            {
                foreach (var slot in formation.Slots)
                {
                    if (previousStarters.TryGetValue(slot.Slot, out var id) && byId.ContainsKey(id) && used.Add(id))
                    {
                        starters[slot.Slot] = id;
                    }
                }
            }

            // 2. Posições vazias: primeiro jogadores dessa posição, depois qualquer um.
            foreach (var slot in formation.Slots.Where(s => !starters.ContainsKey(s.Slot)))
            {
                var pick = available.FirstOrDefault(p => p.Position == slot.Role && !used.Contains(p.Id))
                           ?? available.FirstOrDefault(p => !used.Contains(p.Id));
                if (pick == null)
                {
                    break; // plantel curto: o onze fica incompleto
                }

                used.Add(pick.Id);
                starters[slot.Slot] = pick.Id;
            }

            // 3. Banco: os suplentes anteriores disponíveis e depois os restantes (guarda-redes primeiro).
            var bench = new List<string>();
            foreach (var id in previousBench ?? Array.Empty<string>())
            {
                if (bench.Count < ModelConstants.LineupConst.MaxBench && byId.ContainsKey(id) && used.Add(id))
                {
                    bench.Add(id);
                }
            }

            foreach (var p in available
                         .Where(p => !used.Contains(p.Id))
                         .OrderBy(p => p.Position == Position.GOALKEEPER ? 0 : 1))
            {
                if (bench.Count >= ModelConstants.LineupConst.MaxBench)
                {
                    break;
                }

                used.Add(p.Id);
                bench.Add(p.Id);
            }

            return new AutoLineup(formation.Code, starters, bench);
        }
    }
}
