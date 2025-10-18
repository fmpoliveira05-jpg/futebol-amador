using FluentValidation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Application.DTOs.Team;

namespace Application.Validators
{
    internal class CreateTeamValidator : AbstractValidator<CreateTeamDto>
    {
        /*
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome da equipa é obrigatório")
            .MaximumLength(50).WithMessage("O nome não pode ter mais que 50 caracteres");

        RuleFor(x => x.Localizacao)
            .NotEmpty().WithMessage("A localização é obrigatória");

        RuleFor(x => x.NumeroJogadores)
            .GreaterThanOrEqualTo(5).WithMessage("A equipa deve ter pelo menos 5 jogadores");
    */
    }
}
