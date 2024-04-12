using Microsoft.AspNetCore.Mvc;

namespace FULLSTACK_MTA1_LoginRegister.Controllers.AccountController
{
    using FULLSTACK_MTA1_LoginRegister.Models;
    using Microsoft.AspNetCore.Mvc;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using Microsoft.EntityFrameworkCore;


    public class AccountController : Controller
    {
      
        // User Account Login
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
    //User Account Registration 
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(User user)
    {
        if (ModelState.IsValid)
        {
            if (UserExists(user.Username))
            {
                ModelState.AddModelError("", "Username already exists. Please choose a different one.");
                return View(user);
            }

            if (!IsUsernameValid(user.Username))
            {
                ModelState.AddModelError("", "Username must be at least 6 characters long and cannot contain spaces.");
                return View(user);
            }

            if (!IsPasswordValid(user.Password))
            {
                ModelState.AddModelError("", "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
                return View(user);
            }

            user.Password = HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }
        return View(user);
    }
    // User Login Attempts
    private readonly AppDbContext _context;
    private const int LoginAttempts = 3;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }
