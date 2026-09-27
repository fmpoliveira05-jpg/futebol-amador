using Application.DTOs.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;
using System.Globalization;

namespace Api.Operacao
{
    /// <summary>
    /// Pagina a lista devolvida por uma ação (<c>?page=&amp;pageSize=</c>, máximo 100 por página) e
    /// indica o total em <c>X-Total-Count</c>.
    /// </summary>
    /// <remarks>
    /// Para as listas cujos repositórios ainda não paginam na base de dados (listas de uma equipa ou
    /// de um jogador, limitadas pelo tamanho da equipa). As listas públicas e maiores
    /// (<c>/api/Team/listTeams</c>, <c>/api/Player/listPlayers</c>) paginam no SQL.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PaginarListaAttribute : Attribute, IAsyncResultFilter
    {
        public const string CabecalhoTotal = "X-Total-Count";

        public Task OnResultExecutionAsync(ResultExecutingContext contexto, ResultExecutionDelegate next)
        {
            if (contexto.Result is ObjectResult { Value: IEnumerable lista and not string } resultado &&
                (resultado.StatusCode ?? StatusCodes.Status200OK) == StatusCodes.Status200OK)
            {
                var query = contexto.HttpContext.Request.Query;
                var (saltar, tamanho) = FiltroPaginado.Limites(Ler(query["page"]), Ler(query["pageSize"]));

                var todos = lista.Cast<object?>().ToList();
                contexto.HttpContext.Response.Headers[CabecalhoTotal] = todos.Count.ToString(CultureInfo.InvariantCulture);
                resultado.Value = todos.Skip(saltar).Take(tamanho).ToList();
            }

            return next();
        }

        private static int? Ler(string? valor) =>
            int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
    }
}
