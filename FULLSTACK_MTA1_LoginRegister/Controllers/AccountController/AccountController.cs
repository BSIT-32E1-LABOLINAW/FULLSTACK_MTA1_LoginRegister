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
    private bool UserExists(string username)
    {
        return _context.Users.Any(u => u.Username == username);
    }

    private bool IsUsernameValid(string username)
    {
        return username.Length >= 6 && !username.Contains(" ");
    }

    private int IncrementLoginAttempt()
    {
        var attempts = HttpContext.Session.GetInt32("LoginAttempts") ?? 0;
        attempts++;
        HttpContext.Session.SetInt32("LoginAttempts", attempts);
        return attempts;
    }

    private bool IsPasswordValid(string password)
    {
        var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
        return passwordRegex.IsMatch(password);
    }

    private string HashPassword(string password)
    {
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        return Convert.ToBase64String(hashBytes);
    }

    private bool VerifyPassword(string enteredPassword, string hashedPassword)
    {
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);
        var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        return hashBytes.Skip(16).SequenceEqual(hash);
    }
}