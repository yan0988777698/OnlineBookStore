﻿using Bulky.DataAccess.Repo.IRepo;
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
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            string wwwRootPath = _env.WebRootPath;
            if (files != null)
            {
            //    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            //    string productPath = Path.Combine(wwwRootPath, @"Images\Product", fileName);
            //    //照片更新
            //    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
            //    {
            //        //刪除舊照片
            //        string oldFilePath = Path.Combine(_env.WebRootPath, productVM.Product.ImageUrl.Trim('\\'));
            //        if (System.IO.File.Exists(oldFilePath))
            //        {
            //            System.IO.File.Delete(oldFilePath);
            //        }
            //    }
            //    using (FileStream fs = new FileStream(productPath, FileMode.Create))
            //    {
            //        file.CopyTo(fs);
            //    }
            //    productVM.Product.ImageUrl = @"\Images\Product\" + fileName;
            }
            if (productVM.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
            }
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
            }

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
            //if (!string.IsNullOrEmpty(productToBeDeleted.ImageUrl))
            //{
            //    //刪除舊照片
            //    string oldFilePath = Path.Combine(_env.WebRootPath, productToBeDeleted.ImageUrl.Trim('\\'));
            //    if (System.IO.File.Exists(oldFilePath))
            //    {
            //        System.IO.File.Delete(oldFilePath);
            //    }
            //}
            
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion

    }
}
