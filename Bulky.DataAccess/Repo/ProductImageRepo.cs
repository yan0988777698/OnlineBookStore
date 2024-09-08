using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repo
{
    public class ProductImageRepo : GenericRepo<ProductImage>, IProductImageRepo
    {
        private AppDbContext _db;
        public ProductImageRepo(AppDbContext db) : base(db)
        { 
            _db = db;
        }
        public void Update(ProductImage productImage)
        {
            _db.ProductImages.Update(productImage);
        }
    }
}
