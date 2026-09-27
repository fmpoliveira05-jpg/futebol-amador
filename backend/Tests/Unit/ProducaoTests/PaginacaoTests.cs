using Api.Operacao;
using Application.DTOs.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace UnitTests.Producao
{
    [TestFixture]
    public class PaginacaoTests
    {
        [TestCase(null, null, 0, 50)]
        [TestCase(2, 10, 10, 10)]
        [TestCase(3, 500, 200, 100)]
        [TestCase(0, 0, 0, 1)]
        public void Limites_Aplicam_Predefinicoes_E_Maximo(int? pagina, int? tamanho, int saltar, int esperado)
        {
            var (s, t) = FiltroPaginado.Limites(pagina, tamanho);
            Assert.That(s, Is.EqualTo(saltar));
            Assert.That(t, Is.EqualTo(esperado));
        }

        [Test]
        public void Paginar_Na_Consulta()
        {
            var dados = Enumerable.Range(1, 250).AsQueryable();

            var pagina = dados.Paginar(new FilterTeamDto { Page = 3, PageSize = 100 }).ToList();

            Assert.That(pagina, Is.EqualTo(Enumerable.Range(201, 50)));
        }

        [Test]
        public async Task Atributo_Pagina_A_Resposta_E_Indica_O_Total()
        {
            var http = new DefaultHttpContext();
            http.Request.QueryString = new QueryString("?page=2&pageSize=500");
            var resultado = new OkObjectResult(Enumerable.Range(1, 130).ToList());
            var contexto = new ResultExecutingContext(
                new ActionContext(http, new RouteData(), new ActionDescriptor()),
                new List<IFilterMetadata>(), resultado, controller: new object());

            await new PaginarListaAttribute().OnResultExecutionAsync(contexto,
                () => Task.FromResult<ResultExecutedContext>(null!));

            Assert.That(http.Response.Headers["X-Total-Count"].ToString(), Is.EqualTo("130"));
            Assert.That((System.Collections.IList)resultado.Value!, Has.Count.EqualTo(30));
        }
    }
}
