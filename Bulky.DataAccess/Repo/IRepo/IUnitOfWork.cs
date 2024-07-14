﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repo.IRepo
{
    public interface IUnitOfWork
    {
        ICategoryRepo Category { get; }
        IProductRepo Product { get; }
        ICompanyRepo Company { get; }
        IShoppingCartRepo ShoppingCart { get; }
        IApplicationUserRepo ApplicationUser { get; }
        IOrderHeaderRepo OrderHeader { get; set; }
        IOrderDetailRepo OrderDetail { get; set; }
        void Save();
    }
}
