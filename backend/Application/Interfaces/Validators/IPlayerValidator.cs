using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Validators
{
    internal interface IPlayerValidator
    {
        void PlayerExists(Player player);
    }
}
