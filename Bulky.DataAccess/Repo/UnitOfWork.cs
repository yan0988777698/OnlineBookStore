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
        public IShoppingCartRepo ShoppingCart { get; private set; }
        public IApplicationUserRepo ApplicationUser { get; private set; }
        public IOrderHeaderRepo OrderHeader { get; private set; }
        public IOrderDetailRepo OrderDetail { get; private set; }
        public IProductImageRepo ProductImage { get; private set; }

        private readonly AppDbContext _db;
        public UnitOfWork(AppDbContext db)
        {
            _db = db;
            ApplicationUser = new ApplicationUserRepo(_db);
            Category = new CategoryRepo(_db);
            Company = new CompanyRepo(_db);
            OrderHeader = new OrderHeaderRepo(_db);
            OrderDetail = new OrderDetailRepo(_db);
            Product = new ProductRepo(_db);
            ProductImage = new ProductImageRepo(_db);
            ShoppingCart = new ShoppingCartRepo(_db);
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
