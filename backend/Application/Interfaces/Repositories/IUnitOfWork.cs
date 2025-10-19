using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        /***
         * 0 --> No changed
         * 1 --> Addition
         * 2 --> Delete
         * 3 --> Update
         */
        Task<int> SaveChangesAsync();
    }
}
