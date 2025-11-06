using Application.DTOs.SuperAdmin;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Validators
{
    public interface ISuperAdminValidator
    {
        public void SuperAdminExists(SuperAdmin sadmin);

        public void CreateSuperAdminValidator(CreateSuperAdminDTO dto,User[] sadmins);

        public void GetSuperAdminByIdValidator(SuperAdmin sadmin);

        public void DeleteSuperAdminValidator(SuperAdmin sadmin);

        public void UpdateSuperAdminValidator(UpdateSuperAdminDTO dto,SuperAdmin sadmin, User[] sadmins);
    }
}
