using System.ComponentModel.DataAnnotations;
using Domain.Enums;

/*
 Parei aqui validar se extend está bem, acho que não
 */
namespace Domain.Entities
{
    public class Player : Users
    {
        public Position Position { get; set; }

        public const int minHeight = 100;

        public const int maxHeight = 250;

        [Range(minHeight, maxHeight, ErrorMessage = "Um jogador deve ter entre 100 e 250 centimetors")]
        public int height { get; set; }

        public Teams? Team { get; set; }

        public Guid? idTeam { get; set; } //FK

        public Boolean IsAdmin { get; set; } //Validar se é mesmo necessário

        public ICollection<MembershipRequests> MembershipRequests { get; set; } = new List<MembershipRequests>();
    }
}
