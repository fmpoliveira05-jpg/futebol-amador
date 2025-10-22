using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    internal class PlayerValidator : IPlayerValidator
    {
        public void PlayerExists(Player player)
        {
            if (player == null)
            {
                throw new NotFoundException("O Player não existe");
            }
        }
    }
}
