using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.DTOs.MemberShip
{
    public class MemberShipRequestDto
    {
        public Guid RequestId { get; set; }

        public string PlayerName { get; set; }

        public Guid PlayerId { get; set; }

        public Guid TeamId { get; set; }

        public string TeamName { get; set; }

        public DateTime RequestDate { get; set; }

        public bool IsPlayerSender { get; set; }

    }
}
