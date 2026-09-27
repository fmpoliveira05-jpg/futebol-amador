using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Filters
{
    /// <summary>
    /// Paginação das listas: <c>?page=1&amp;pageSize=50</c>. Sem parâmetros, devolve a primeira página
    /// com <see cref="TamanhoPredefinido"/> elementos; nunca mais de <see cref="TamanhoMaximo"/>.
    /// </summary>
    public class FiltroPaginado
    {
        public const int TamanhoPredefinido = 50;
        public const int TamanhoMaximo = 100;

        /// <summary>Página (a partir de 1).</summary>
        [Range(1, 100_000, ErrorMessage = "A página tem de ser um número entre {1} e {2}.")]
        public int? Page { get; set; }

        /// <summary>Elementos por página (1 a 100).</summary>
        [Range(1, TamanhoMaximo, ErrorMessage = "O tamanho da página tem de estar entre {1} e {2}.")]
        public int? PageSize { get; set; }

        /// <summary>Número de elementos a saltar e a devolver, já com os limites aplicados.</summary>
        public static (int Saltar, int Tamanho) Limites(int? pagina, int? tamanho)
        {
            var t = Math.Clamp(tamanho ?? TamanhoPredefinido, 1, TamanhoMaximo);
            var p = Math.Max(1, pagina ?? 1);
            return ((p - 1) * t, t);
        }
    }

    public static class PaginacaoExtensions
    {
        /// <summary>Aplica a página na consulta (a ordenação tem de vir antes, para ser estável).</summary>
        public static IQueryable<T> Paginar<T>(this IQueryable<T> consulta, FiltroPaginado? filtro)
        {
            var (saltar, tamanho) = FiltroPaginado.Limites(filtro?.Page, filtro?.PageSize);
            return consulta.Skip(saltar).Take(tamanho);
        }
    }
}
