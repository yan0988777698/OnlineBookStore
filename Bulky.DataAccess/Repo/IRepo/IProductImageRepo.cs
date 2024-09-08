using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repo.IRepo
{
    public interface IProductImageRepo : IGenericRepo<ProductImage>
    {
        void Update(ProductImage productImage);
    }
}
