using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _db;
        public CategoryController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult List()
        {
            List<Category> categoryList = _db.Categories.ToList();
            return View(categoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {

            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "名稱與數量不能相同");
            }
            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
                TempData["success"] = "Category created seccessfully";
                return RedirectToAction("List", "Category");
            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            //Find只針對Primary key進行搜尋
            Category? categoryFormDB = _db.Categories.Find(id);
            //FirstOrDefault可以自訂搜尋對象，可以搭配Where進行資料篩選
            Category? categoryFormDB2 = _db.Categories.FirstOrDefault(x=>x.Id == id);
            Category? categoryFormDB3 = _db.Categories.Where(x=>x.Id == id).FirstOrDefault();


            if (categoryFormDB == null)
                return NotFound();

            return View(categoryFormDB);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(category);
                _db.SaveChanges();
                TempData["success"] = "Category updated seccessfully";
                return RedirectToAction("List", "Category");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
       
            Category? categoryFormDB = _db.Categories.Find(id);

            if (categoryFormDB == null)
                return NotFound();

            return View(categoryFormDB);
        }
        [HttpPost,ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Category? category = _db.Categories.Find(id);
            if(category is null)
                return NotFound();
            _db.Categories.Remove(category);
            _db.SaveChanges();
            TempData["success"] = "Category deleted seccessfully";
            return RedirectToAction("List", "Category");
        }
    }
}
