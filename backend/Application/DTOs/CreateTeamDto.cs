using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
DTOs são Objetos usados para comunicação entre API e frontends, evitando expor 
entidades do domínio diretamente
 * */
namespace Application.DTOs
{
    //Por resto das caracteristicas
    public class CreateTeamDto
    {
        public string Nome { get; set; }
        public string Localizacao { get; set; }
        public List<Guid> CoAdministradores { get; set; }
    }
}
