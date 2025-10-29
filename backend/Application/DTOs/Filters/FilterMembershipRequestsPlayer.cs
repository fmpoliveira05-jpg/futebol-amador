using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Filters
{
    public class FilterMembershipRequestsPlayer
    {
        public string? SenderName { get; set; }

        public DateOnly? MinDate { get; set; }

        public DateOnly? MaxDate { get; set; }
    }
}