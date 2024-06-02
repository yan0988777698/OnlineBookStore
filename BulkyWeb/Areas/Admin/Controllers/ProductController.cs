using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult List()
        {
            List<Product> products = _unitOfWork.Product.GetAll().ToList();
            return View(products);
        }
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(x =>
            new SelectListItem { Text = x.Name, Value = x.Id.ToString() }); 

            ProductVM productVM = new (){
                Product = new Product(),
                CategoryList = CategoryList
            };
            if(id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            _unitOfWork.Product.Add(productVM.Product);
            _unitOfWork.Save();
            TempData["success"] = "商品新增成功";
            return RedirectToAction("List");
        }
 
        public IActionResult Delete(int id)
        {
            Product product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            Product? product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "商品刪除成功";
            return RedirectToAction("List");
        }
    }
}
