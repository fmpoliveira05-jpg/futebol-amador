using System.ComponentModel.DataAnnotations;

/// <summary>
/// Representa a entidade que gere o calendário e a lista de partidas agendadas ou realizadas de uma equipa.
/// Esta entidade é um contentor (container) para as partidas.
/// </summary>
namespace Domain.Entities
{
    public class Calendar
    {
        /// <summary>
        /// O identificador único (GUID) do Calendário.
        /// Anotado com [Key] para ser a chave primária na base de dados (Code-First).
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();


        /// <summary>
        /// Coleção de partidas associadas a este calendário.
        /// Esta é uma chave estrangeira (FK) que representa uma relação One-to-Many.
        /// </summary>
        public ICollection<Matches> Matches { get; set; } = new List<Matches>(); //FK


        /// <summary>
        /// Construtor padrão da entidade Calendar.
        /// Inicializa a coleção de partidas como uma lista vazia.
        /// </summary>
        public Calendar()
        {
            this.Matches = new List<Matches>();
        }
    }
}
