using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

/*
 Parei aqui validar se extend está bem, acho que não
 */
namespace Domain.Entities
{
    internal class Player : Users
    {
        public Postion Position { get; set; }

        public static int minHeight = 100;

        public static int maxHeight = 250;

        [Range(minHeight, maxHeight), ErrorMessage = "Um jogador deve ter entre 100 e 250 centimetors"]
        public int height { get; set; }

        public Teams? Team { get; set; }

        public Boolean IsAdmin { get; set; } //Validar se é mesmo necessário

        public ICollection<MembershipRequests> MembershipRequests { get; set; } = new List<MembershipRequests>();
    }
}
