using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationAspDotnetCore.Data;
using AuthenticationAspDotnetCore.Models;
using AuthenticationAspDotnetCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAspDotnetCore.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // take current login user id
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            var userList = _db.ApplicationUsers.Where(u => u.Id != claims.Value);
            
            foreach (var user in userList)
            {
                var userTemp = await _userManager.FindByIdAsync(user.Id);
                var roleTemp = await _userManager.GetRolesAsync(userTemp);
                user.Role = roleTemp.FirstOrDefault();
            }

            if (User.IsInRole("Staff"))
            {
                return View(userList.ToList().Where(u => u.Role != "Admin"));
            }
            
            return View(userList);
        }

        [HttpGet]
        public IActionResult Update(string id)
        {
            if (id != null)
            {
                var user = _db.ApplicationUsers.Find(id);
                return View(user);
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult Update(ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                var user = _db.ApplicationUsers.Find(applicationUser.Id);
                user.FullName = applicationUser.FullName;
                user.DateOfBirth = applicationUser.DateOfBirth;
                user.HealthCareId = applicationUser.HealthCareId;
                user.CredentialId = applicationUser.CredentialId;

                _db.ApplicationUsers.Update(user);
                _db.SaveChanges();
                
                return RedirectToAction(nameof(Index));
            }

            return View(applicationUser);
        }
        
        [HttpGet]
        public async Task<IActionResult> LockUnlock(string id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var userNeedToLock = _db.ApplicationUsers.Where(u => u.Id == id).First();

            if (userNeedToLock.Id == claims.Value)
            {
                // hieen ra loi ban dang khoa tai khoan cua chinh minh
            }

            if (userNeedToLock.LockoutEnd != null && userNeedToLock.LockoutEnd > DateTime.Now)
            {
                userNeedToLock.LockoutEnd = DateTime.Now;
            }
            else
            {
                userNeedToLock.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            await _userManager.DeleteAsync(user);
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            var user = _db.ApplicationUsers.Find(id);

            if (user == null)
            {
                return View();
            }

            ConfirmEmailVM confirmEmailVm = new ConfirmEmailVM()
            {
                Email = user.Email
            };

            return View(confirmEmailVm);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailVM confirmEmailVm)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(confirmEmailVm.Email);
                if ( user!= null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    return RedirectToAction("ResetPassword", "Users", new { token = token, email = user.Email });
                }
            }
            return View(confirmEmailVm);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {

            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }

            ResetPasswordViewModel resetPasswordViewModel = new ResetPasswordViewModel()
            {
                Email = email,
                Token = token
            };

            return View(resetPasswordViewModel);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token,
                        resetPasswordViewModel.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            return View(resetPasswordViewModel);
        }
    }
}