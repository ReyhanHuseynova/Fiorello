using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Areas.admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            List<AppUser> appUsers = await _userManager.Users.ToListAsync();
            List<UserVM> userVMs = new List<UserVM>();
            foreach (AppUser user in appUsers)
            {
                UserVM userVM = new UserVM
                {
                    Id = user.Id,
                    Fullname = user.Name + user.Surname,
                    Username = user.UserName,
                    Email = user.Email,
                    IsDeactive = user.IsDeactive,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                };
                userVMs.Add(userVM);

            }
            return View(userVMs);


        }

        public  IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterVM registerVM, string roleName)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser appUser = new AppUser
            {
                Name = registerVM.Name,
                Email = registerVM.Email,
                Surname = registerVM.Surname,
                UserName = registerVM.Username


            };



            IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            await _userManager.AddToRoleAsync(appUser, roleName);
           
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Activity(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
           AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            if (user.IsDeactive == true)
            {
                user.IsDeactive = false;
            }
            else
            {
                user.IsDeactive = true;
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }
  
      
        public async Task<IActionResult> Update(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
         
            UpdateVM updateVM= new UpdateVM
            {

                Name = user.Name,
                Email = user.Email,
                Surname = user.Surname,
                Username = user.UserName,
                Role= (await _userManager.GetRolesAsync(user)).First()
            };
            return View(updateVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, UpdateVM updateVM, string roleName)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            string oldRole = (await _userManager.GetRolesAsync(user)).First();

            UpdateVM dbUpdateVM = new UpdateVM
            {

                Name = user.Name,
                Email = user.Email,
                Surname = user.Surname,
                Username = user.UserName,
                Role = oldRole
            };

            user.Email= dbUpdateVM.Email;   
            user.Name= dbUpdateVM.Name;   
            user.Surname= dbUpdateVM.Surname;   
            user.UserName= dbUpdateVM.Username;

           IdentityResult reIdentityResult = await _userManager.RemoveFromRoleAsync(user, oldRole);
            if(!reIdentityResult.Succeeded)
            {
                foreach (IdentityError error in reIdentityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
           IdentityResult addIdentityResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addIdentityResult.Succeeded)
            {
                foreach (IdentityError error in addIdentityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ResetPassword(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }

           
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, ResetPasswordVM reset)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            string token= await _userManager.GeneratePasswordResetTokenAsync(user);
           IdentityResult identityResult= await _userManager.ResetPasswordAsync(user, "token ", reset.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            return RedirectToAction("Index");
        }


    }
}
