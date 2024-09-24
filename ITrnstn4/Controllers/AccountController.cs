using ITrnstn4.Data;
using ITrnstn4.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ITrnstn4Old.Data;
using System;

namespace ITrnstn4Old.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult RedirectAuthUser(string action)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("UserManager");
            }
            return View();
        }

        public User StartUser(User user)
        {
            user.Password = PasswordHasher.HashPassword(user.Password);
            user.RegistrationDate = DateTime.Now;

            return user;
        }

        public void ChangeUserStatus(User user, string status)
        {
            user.Status = status;
            _context.SaveChanges();
        }

        public User StartUserSession(User user)
        {
            user.LastLoginDate = DateTime.Now;
            _context.SaveChanges();

            if (user.Status == "Blocked") ChangeUserStatus(user, "Active");

            return user;
        }

        public void AddToDb(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public ClaimsIdentity GetClaimsIdentity(User user)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Nickname), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };

            return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public AuthenticationProperties GetAuthProperties()
        {
            return new AuthenticationProperties { IsPersistent = true };
        }

        public async Task<RedirectToActionResult> SetAuthenticationAsync(User user)
        {
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(GetClaimsIdentity(user)), GetAuthProperties());

            return RedirectToAction("UserManager", "Account");
        }

        public bool TryRegister(User user)
        {
            try
            {
                AddToDb(StartUser(user));

                return true;
            }
            catch (Exception ex)
            {
                new DbErrorHandler().HandleDatabaseError(ex, ModelState);

                return false;
            }
        }

        public async Task<IActionResult> AuthenticateUserAsync(User user)
        {
            StartUserSession(user);

            await SetAuthenticationAsync(user);

            return RedirectToAction("UserManager", "Account");
        }

        public IActionResult ReportFailedAuth(string failMessage)
        {
            ModelState.AddModelError("", failMessage);
            return View();
        }

        public async Task<IActionResult> DoUserActionAsync(List<User> usersToDo, string action)
        {
            string rediectAction = "UserManager";

            foreach (var user in usersToDo)
            {
                switch (action)
                {
                    case "block":
                        await BlockUser(user.Id);
                        rediectAction = "Login";
                        break;
                    case "unblock":
                        ChangeUserStatus(user, "Active");
                        break;
                    case "delete":
                        await DeleteUser(user.Id);
                        rediectAction = "Register";
                        break;
                    default: break;
                }
            }

            return RedirectToAction(rediectAction, "Account");
        }

        public async Task<IActionResult> StopUserSessionAsync(int userId, string action)
        {
            if (userId.ToString() == User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return await Logout(action);
            }

            return RedirectToAction("UserManager", "Account");
        }

        public void RemoveUser(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public async Task<IActionResult> IsUserVerifiedAsync(User user, string password)
        {
            string message = "Invalid e-mail or password";

            if (user != null && PasswordHasher.VerifyPassword(password, user))
            {
                if (user.Status == "Active") return await AuthenticateUserAsync(user);
                else message = "Sorry, your account was blocked.";
            }

            return ReportFailedAuth(message);
        }

        public async Task<IActionResult> Logout(string action)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(action, "Account");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return RedirectAuthUser("UserManager");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid && TryRegister(user))
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return RedirectAuthUser("UserManager");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);

            return await IsUserVerifiedAsync(user, password);
        }

        [Authorize]
        [HttpGet]
        public IActionResult UserManager()
        {
            var users = _context.Users.ToList();

            return View(users);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> BulkActionAsync(List<int> selectedUsers, string action)
        {
            if (selectedUsers != null)
            {
                var usersToDo = _context.Users.Where(u => selectedUsers.Contains(u.Id)).ToList();

                if (usersToDo.Count != 0)
                {
                    return await DoUserActionAsync(usersToDo, action);
                }
            }

            return RedirectToAction("UserManager", "Account");
        }

        public async Task BlockUser(int userId)
        {
            var user = _context.Users.SingleOrDefault(u => u.Id == userId);

            if (user != null)
            {
                ChangeUserStatus(user, "Blocked");
                
                await StopUserSessionAsync(userId, "Login");
            }
        }

        public async Task DeleteUser(int userId)
        {
            var user = _context.Users.SingleOrDefault(u => u.Id == userId);

            if (user != null)
            {
                RemoveUser(user);

                await StopUserSessionAsync(userId, "Register");
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
