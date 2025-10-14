using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/*
 Parei aqui validar se extend está bem, acho que não
 */
namespace Domain.Entities
{
    internal class Calendar
    {
        [Key]
        [MinLength = 32, MaxLength = 36]
        public string Id { get; set; } //PK

        public ICollection<Matches> Matches { get; set; } = new List<Matches>() //FK
    }
}
