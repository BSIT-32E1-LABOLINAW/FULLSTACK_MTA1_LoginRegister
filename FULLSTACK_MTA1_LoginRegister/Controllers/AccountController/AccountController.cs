using Microsoft.AspNetCore.Mvc;

namespace FULLSTACK_MTA1_LoginRegister.Controllers.AccountController
{
    using FULLSTACK_MTA1_LoginRegister.Models;
    using Microsoft.AspNetCore.Mvc;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;


    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private const int LoginAttempts = 3;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // LOGIN SECTION
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            var loggedInUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);

            if (loggedInUser != null && VerifyPassword(user.Password, loggedInUser.Password))
            {
                HandleSuccessfulLogin(loggedInUser.Username);
                return RedirectToAction("Index", "Home");
            }

            HandleFailedLogin();
            return View(user);
        }

        private void HandleSuccessfulLogin(string username)
        {
            HttpContext.Session.SetString("Username", username);
            HttpContext.Session.SetInt32("LoginAttempts", 0);
        }

        private void HandleFailedLogin()
        {
            var remainingAttempts = LoginAttempts - IncrementLoginAttempt();
            if (remainingAttempts > 0)
                ModelState.AddModelError("", $"Invalid username or password. {remainingAttempts} attempts remaining.");
            else
                ModelState.AddModelError("", $"You have exceeded the maximum number of login attempts. Please try again later or re-register.");
        }
    }
