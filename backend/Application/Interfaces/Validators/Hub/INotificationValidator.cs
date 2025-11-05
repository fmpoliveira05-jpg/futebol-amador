using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Validators.Hub
{
    public interface INotificationValidator
    {
        public Task ValidateTeamMembershipAsync(Guid teamId, string? userId);
    }
}
