using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repo;
using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult List()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().ToList();
            return View(companyList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Company company)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Company.Add(company);
                _unitOfWork.Save();
                TempData["success"] = "公司新增成功";
                return RedirectToAction("List", "Company");
            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            Company? companyFormDB = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);

            if (companyFormDB == null)
                return NotFound();

            return View(companyFormDB);
        }
        [HttpPost]
        public IActionResult Edit(Company company)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Company.Update(company);
                _unitOfWork.Save();
                TempData["success"] = "公司更新成功";
                return RedirectToAction("List", "Company");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            Company? companyFormDB = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);

            if (companyFormDB == null)
                return NotFound();

            return View(companyFormDB);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Company? companyFormDB = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
            if (companyFormDB is null)
                return NotFound();
            _unitOfWork.Company.Remove(companyFormDB);
            _unitOfWork.Save();
            TempData["success"] = "公司刪除成功";
            return RedirectToAction("List", "Company");
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }
        [HttpDelete]
        public IActionResult DeleteById(int? id)
        {
            var company = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
            if (company is null)
                return Json(new { success = false, message = "公司不存在" });
            _unitOfWork.Company.Remove(company);
            _unitOfWork.Save();
            return Json(new { success = true, message = "公司刪除成功" });
        }
    }
}
