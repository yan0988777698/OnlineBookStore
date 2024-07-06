using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repo.IRepo
{
    public interface ICompanyRepo : IGenericRepo<Company>
    {
        void Update(Company company);
    }
}
