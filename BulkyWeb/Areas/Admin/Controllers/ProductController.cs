using Bulky.DataAccess.Repo;
using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
        }
        public IActionResult List()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(products);
        }
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(x =>
            new SelectListItem { Text = x.Name, Value = x.Id.ToString() });

            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = CategoryList
            };
            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id, includeProperties: "ProductImages");
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            string wwwRootPath = _env.WebRootPath;

            if (productVM.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
            }
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
            }

            _unitOfWork.Save();
            if (files != null)
            {
                foreach (IFormFile file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = @"Images\Products\Product-" + productVM.Product.Id;
                    string finalPath = Path.Combine(wwwRootPath, productPath);

                    if (!Directory.Exists(finalPath))
                        Directory.CreateDirectory(finalPath);
                    using (FileStream fs = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }
                    ProductImage image = new()
                    {
                        ImageUrl = @"\" + Path.Combine(productPath, fileName),
                        ProductId = productVM.Product.Id,
                    };
                    if (productVM.Product.ProductImages == null)
                        productVM.Product.ProductImages = new List<ProductImage>();

                    productVM.Product.ProductImages.Add(image);
                }
                _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
            }
            TempData["success"] = "商品新增/更新成功";
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

        public IActionResult DeleteImage(int ImageId)
        {
            ProductImage imageToBeDeleted = _unitOfWork.ProductImage.GetFirstOrDefault(x => x.Id == ImageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var filePath = Path.Combine(_env.WebRootPath, imageToBeDeleted.ImageUrl.Trim('\\'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                }
                _unitOfWork.Save();
            }
            TempData["success"] = "刪除成功";
            return RedirectToAction("Upsert", new { id = productId });
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = products });
        }
        [HttpDelete]
        public IActionResult DeleteById(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Product is not found." });
            }
            string productPath = @"Images\Products\Product-" + id;
            string finalPath = Path.Combine(_env.WebRootPath, productPath);
            if (Directory.Exists(finalPath))
            {

                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO .File.Delete(filePath);
                }
                Directory.Delete(finalPath);
            }
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion

    }
}
