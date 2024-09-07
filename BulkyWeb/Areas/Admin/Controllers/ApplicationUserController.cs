using Bulky.DataAccess.Data;
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
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<IdentityUser> _userManager;
        public ApplicationUserController(AppDbContext appDbContext, UserManager<IdentityUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RoleManagement(string id)
        {
            string roleId = _appDbContext.UserRoles.FirstOrDefault(x => x.UserId == id).RoleId;
            List<Company> companyList = _appDbContext.Companies.ToList();

            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                User = _appDbContext.ApplicationUsers.Include(x => x.Company).FirstOrDefault(x => x.Id == id),
                RoleList = _appDbContext.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name,
                }),
                CompanyList = _appDbContext.Companies.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                }),
            };
            roleManagementVM.User.Role = _appDbContext.Roles.FirstOrDefault(x => x.Id == roleId).Name;
            return View(roleManagementVM);
        }
        [HttpPost]
        public IActionResult RoleManagementPOST(RoleManagementVM vm)
        {
            string roleId = _appDbContext.UserRoles.FirstOrDefault(x => x.UserId == vm.User.Id).RoleId;
            string oldRole = _appDbContext.Roles.FirstOrDefault(x => x.Id == roleId).Name;
            
            if(vm.User.Role != oldRole)
            {
                ApplicationUser user = _appDbContext.ApplicationUsers.FirstOrDefault(x => x.Id == vm.User.Id);
                if(vm.User.Role == SD.Role_Company)
                {
                    user.CompanyId = vm.User.CompanyId;
                }
                if(oldRole == SD.Role_Company)
                {
                    user.CompanyId = null;
                }
                _appDbContext.SaveChanges();

                _userManager.RemoveFromRoleAsync(user, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, vm.User.Role).GetAwaiter().GetResult();

            }
            
            return RedirectToAction("Index");
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> applicationUsersList = _appDbContext.ApplicationUsers.Include(x => x.Company).ToList();
            var userRoles = _appDbContext.UserRoles.ToList();
            var roles = _appDbContext.Roles.ToList();

            foreach (var user in applicationUsersList)
            {
                string roleId = userRoles.FirstOrDefault(x => x.UserId == user.Id).RoleId;
                string role = roles.FirstOrDefault(x => x.Id == roleId).Name;
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
            var dataFromDb = _appDbContext.ApplicationUsers.FirstOrDefault(x => x.Id == id);
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
            _appDbContext.SaveChanges();

            return Json(new { success = true, message = "更新成功" });
        }
        #endregion
    }
}
