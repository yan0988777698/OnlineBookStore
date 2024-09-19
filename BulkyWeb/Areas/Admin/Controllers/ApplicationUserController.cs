using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ApplicationUserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ApplicationUserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RoleManagement(string id)
        {
            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                User = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id,includeProperties: "Company"),
                RoleList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name,
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                }),
            };
            roleManagementVM.User.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id))
                                        .GetAwaiter().GetResult().FirstOrDefault();
            return View(roleManagementVM);
        }
        [HttpPost]
        public IActionResult RoleManagementPOST(RoleManagementVM vm)
        {
            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == vm.User.Id))
                            .GetAwaiter().GetResult().FirstOrDefault();
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == vm.User.Id);
            if (vm.User.Role != oldRole)
            {
                if(vm.User.Role == SD.Role_Company)
                {
                    user.CompanyId = vm.User.CompanyId;
                }
                if(oldRole == SD.Role_Company)
                {
                    user.CompanyId = null;
                }
                //_unitOfWork.ApplicationUser.Update(user);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(user, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, vm.User.Role).GetAwaiter().GetResult();

            }
            else
            {
                if(oldRole == SD.Role_Company && user.CompanyId != vm.User.CompanyId)
                {
                    user.CompanyId = vm.User.CompanyId;
                    //_unitOfWork.ApplicationUser.Update(user);
                    _unitOfWork.Save();
                }
            }
            
            return RedirectToAction("Index");
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> applicationUsersList = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();

            foreach (var user in applicationUsersList)
            {
                string role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                user.Role = role;

                if (user.Company == null)
                {
                    user.Company = new Company() { Name = "" };
                }
            }

            return Json(new { data = applicationUsersList });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var dataFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);
            if (dataFromDb == null)
            {
                return Json(new { success = "false", message = "Error while Locking/Unlocking" });
            }
            if (dataFromDb.LockoutEnd != null && dataFromDb.LockoutEnd > DateTime.Now)
            {
                dataFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                dataFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            //_unitOfWork.ApplicationUser.Update(dataFromDb);
            _unitOfWork.Save();

            return Json(new { success = true, message = "更新成功" });
        }
        #endregion
    }
}
