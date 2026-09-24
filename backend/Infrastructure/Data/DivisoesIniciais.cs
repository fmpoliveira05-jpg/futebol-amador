using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <summary>
    /// Cria as divisões (ranks) numa base de dados nova. Sem elas não é possível criar equipas:
    /// cada equipa começa na divisão mais baixa (a que não tem divisão anterior).
    /// </summary>
    public static class DivisoesIniciais
    {
        /// <summary>Nome, pontos por vitória/empate/derrota e pontos para subir.</summary>
        private static readonly (string Nome, int Vitoria, int Empate, int Derrota, int ParaSubir)[] Divisoes =
        {
            ("Divisão 4", 30, 10, -5, 300),
            ("Divisão 3", 25, 8, -8, 700),
            ("Divisão 2", 20, 6, -10, 1200),
            ("Divisão 1", 15, 5, -12, int.MaxValue),
        };

        /// <summary>Não faz nada se já existirem divisões.</summary>
        public static async Task GarantirAsync(AmateurFootballContext db, CancellationToken ct = default)
        {
            if (await db.Rank.AnyAsync(ct))
            {
                return;
            }

            var ranks = Divisoes
                .Select(d => new Rank
                {
                    Name = d.Nome,
                    WinPoints = d.Vitoria,
                    DrawPoints = d.Empate,
                    LosePoints = d.Derrota,
                    PointsToPromotion = d.ParaSubir,
                })
                .ToList();

            // Primeiro as divisões e só depois as ligações entre elas: cada divisão aponta para a
            // seguinte e para a anterior, e o EF não consegue inserir essa dependência circular
            // numa única gravação.
            db.Rank.AddRange(ranks);
            await db.SaveChangesAsync(ct);

            for (var i = 0; i < ranks.Count; i++)
            {
                ranks[i].IdPreviousRank = i > 0 ? ranks[i - 1].Id : null;
                ranks[i].IdNextRank = i < ranks.Count - 1 ? ranks[i + 1].Id : null;
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
