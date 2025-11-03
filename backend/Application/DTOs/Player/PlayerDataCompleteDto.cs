using Application.DTOs.PlayerDTOs;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Player
{
    public class PlayerDataCompleteDto
    {
        [MaxLength(256)]
        public required string email { get; set; }

        //quando remover a data de nascimento do playerdetails passar para ca

        public required PlayerDetailsDTO PlayerDetails { get; set; }
    }
}
