using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repo;
using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ApplicationUserController : Controller
    {
        private readonly AppDbContext _appDbContext;
        public ApplicationUserController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public IActionResult Index()
        {
            return View();
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
        public IActionResult LockUnlock([FromBody]string id)
        {
            var dataFromDb = _appDbContext.ApplicationUsers.FirstOrDefault(x => x.Id == id);
            if(dataFromDb == null)
            {
                return Json(new { success = "false", message = "Error while Locking/Unlocking" });
            }
            if(dataFromDb.LockoutEnd != null && dataFromDb.LockoutEnd > DateTime.Now)
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
