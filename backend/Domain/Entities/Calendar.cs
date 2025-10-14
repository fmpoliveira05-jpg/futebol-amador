using System.ComponentModel.DataAnnotations;

/*
 Parei aqui validar se extend está bem, acho que não
 */
namespace Domain.Entities
{
    public class Calendar
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public ICollection<Matches> Matches { get; set; } = new List<Matches>(); //FK
    }
}
