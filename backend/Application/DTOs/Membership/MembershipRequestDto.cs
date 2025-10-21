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

        public string TeamName { get; set; }
        public DateTime RequestDate { get; set; }

        public IEnumerable<MemberShipRequestDto> ToDtoList(IEnumerable<MembershipRequests> requests)
        {
            return requests.Select(r => new MemberShipRequestDto
            {
                RequestId = r.Id,
                PlayerName = r.Player.Name,
                PlayerId = r.Player.Id,
                TeamName = r.Team.Name,
                RequestDate = r.InviteDate
            });
        }
    }
}
