using System;
using System.Linq;
using AuthenticationAspDotnetCore.Data;
using AuthenticationAspDotnetCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAspDotnetCore.Initializer
{
    public class DbInitailizer: IDbInitailizer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        

        public DbInitailizer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public void Initialize()
        {
            // check non migration if not conduct migration
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch(Exception ex)
            {
                
            }

            if (_db.Roles.Any(r => r.Name == "Admin")) return;
            if (_db.Roles.Any(r => r.Name == "Staff")) return;
            
            _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole("Staff")).GetAwaiter().GetResult();
            
            // create admin and add to role admin
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
                FullName = "Admin"
            },"Admin123@").GetAwaiter().GetResult() ;

            ApplicationUser userAdmin = _db.ApplicationUsers.Where(u => u.Email == "admin@gmail.com").FirstOrDefault();

            _userManager.AddToRoleAsync(userAdmin,"Admin").GetAwaiter().GetResult();
            // create Staff and add to role staff
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "Staff@gmail.com",
                Email = "Staff@gmail.com",
                EmailConfirmed = true,
                FullName = "Staff"
            },"Staff123@").GetAwaiter().GetResult() ;

            ApplicationUser userStaff = _db.ApplicationUsers.Where(u => u.Email == "Staff@gmail.com").FirstOrDefault();

            _userManager.AddToRoleAsync(userStaff,"Staff").GetAwaiter().GetResult();
        }
    }
}