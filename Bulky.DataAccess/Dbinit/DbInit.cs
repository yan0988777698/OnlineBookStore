using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DbInit
{
    public class DbInit : IDbInit
    {
        private readonly AppDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public DbInit(AppDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void initialize()
        {
            //migrations if they are not applied
            try
            {
                if (_dbContext.Database.GetPendingMigrations().Count() > 0)
                {
                    _dbContext.Database.Migrate();
                }

            }
            catch (Exception ex)
            {

            }
            //create roles if they are not created
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();

                //if roles are not created, then we will create admin user as well
                _userManager.CreateAsync(new ApplicationUser
                {
                    Name = "Admin",
                    UserName = "Admin@gmail.com",
                    Email = "Admin@gmail.com",
                    PhoneNumber = "0911222333",
                    StreetAddress = "City Hall Rd.",
                    Region = "Xinyi District",
                    City = "Taipei City",
                    PostalCode = "110"
                }, "Admin123+").GetAwaiter().GetResult();
                ApplicationUser user = _dbContext.ApplicationUsers.FirstOrDefault(x => x.UserName == "Admin@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
