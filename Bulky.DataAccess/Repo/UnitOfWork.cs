using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repo
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepo Category { get; private set; }
        public IProductRepo Product { get; private set; }
        public ICompanyRepo Company { get; private set; }
        public IShoppingCartRepo ShoppingCart { get; set; }
        public IApplicationUserRepo ApplicationUser { get; set; }
        public IOrderHeaderRepo OrderHeader { get; set; }
        public IOrderDetailRepo OrderDetail { get; set; }

        private readonly AppDbContext _db;
        public UnitOfWork(AppDbContext db)
        {
            _db = db;
            Category = new CategoryRepo(_db);
            Product = new ProductRepo(_db);
            Company = new CompanyRepo(_db);
            ShoppingCart = new ShoppingCartRepo(_db);
            ApplicationUser = new ApplicationUserRepo(_db);
            OrderHeader = new OrderHeaderRepo(_db);
            OrderDetail = new OrderDetailRepo(_db);
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
