using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repo
{
    public class CategoryRepo : GenericRepo<Category>, ICategoryRepo
    {
        private readonly AppDbContext _db;
        public CategoryRepo(AppDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
