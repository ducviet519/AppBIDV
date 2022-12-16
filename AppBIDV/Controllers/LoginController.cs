using BC = BCrypt.Net.BCrypt;
using DataBIDV.Models;
using DataBIDV.Services.Interfaces;
using AppBIDV.Authorizations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace AppBIDV.Controllers
{
    public class LoginController : Controller
    {
        #region UserHelper
        private readonly IUnitOfWork _services;
        public LoginController(IUnitOfWork services)
        {
            _services = services;
        }
        private async Task<List<Claim>> GenerateClaim(UserModel user)
        {
            //var UserLoginInfo = await _services.Permission.GetUsersByNameOrID(user.Username);
            //var RoleInUser = await _services.Permission.GetRoleInUser(user.Username);
            //var PermissionMenusInUser = await _services.Permission.GetMenuPermissionsInUser(user.Username);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.DisplayName ?? "Khách"),
                new Claim(ClaimTypes.NameIdentifier, user.Username ?? "Unknow"),
                new Claim(ClaimTypes.GroupSid, user.Source ?? "local"),
                new Claim(ClaimTypes.Email, user.EmailAddress ?? ""),
                new Claim(ClaimTypes.GivenName, user.Code ?? ""),
            };
            //foreach (var role in RoleInUser)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
            //}
            //foreach (var permission in PermissionMenusInUser)
            //{
            //    claims.Add(new Claim("Permission", $"{permission.NavigationMenuName}"));
            //}
            return claims;
        }
        private UserModel GetUser(UserLogin login)
        {
            try
            {
                string domain = "bvta.vn";
                var userprincipal = $@"{domain}\{login.Username}";
                using PrincipalContext context = new PrincipalContext(ContextType.Domain, domain, userprincipal, login.Password);                
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, login.Username);
                if (user == null)
                {
                    return null;
                }
                else
                {
                    UserModel userAccount = new UserModel()
                    {
                        DisplayName = user.DisplayName,
                        Username = user.SamAccountName,
                        Password = login.Password,
                        Source = domain,
                        EmailAddress = user.UserPrincipalName,
                        Code = user.Description,
                        Status = (user.Enabled ?? false)
                    };

                    //Ghi thông tin người dùng vào CSDL
                    Users users = new Users()
                    {
                        DisplayName = user.DisplayName,
                        UserName = user.SamAccountName,
                        Password = BC.HashPassword(login.Password),
                        Source = domain,
                        Email = user.UserPrincipalName,
                        Code = user.Description,
                        Status = (user.Enabled ?? false)
                    };
                    //_services.Permission.InsertUsers(users);
                    return userAccount;
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                return null;
            }
        }

        //private async Task<bool> Authenticate(UserLogin user)
        //{
        //    var account = await _services.Permission.GetUsersByNameOrID(user.Username);
        //    if (account == null || !BC.Verify(user.Password, account.Password))
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
        #endregion

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewData["RerurnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(UserLogin login)
        {
            if (String.IsNullOrEmpty(login.Username) || String.IsNullOrEmpty(login.Password))
            {
                TempData["Error"] = "Lỗi! Tài khoản hoặc mật khẩu không được bỏ trống";
                return View(login);
            }
            try
            {
                var UserLoginInfo = GetUser(login);
                if (UserLoginInfo != null)
                {
                    string adPath = $"LDAP://{UserLoginInfo.Source}";
                    var ad_authenticate = new ActiveDirectoryHelper(adPath);
                    //if (ad_authenticate.IsAuthenticated(UserLoginInfo.Source, UserLoginInfo.Username, UserLoginInfo.Password) && (await Authenticate(login)))
                    if (ad_authenticate.IsAuthenticated(UserLoginInfo.Source, UserLoginInfo.Username, UserLoginInfo.Password))
                    {
                        List<Claim> claims = await GenerateClaim(UserLoginInfo);
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
                            new AuthenticationProperties()
                            {
                                //IsPersistent = UserLoginInfo.Status
                            });
                        string returnUrl = Request.Form["ReturnUrl"];
                        if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);
                        else
                            return RedirectToAction(nameof(HomeController.Index), "Home");
                    }
                    TempData["Error"] = "Lỗi! Tài khoản hoặc Mật khẩu không chính xác.";
                    return View();
                }
                else
                {
                    TempData["Error"] = "Lỗi! Tài khoản hoặc Mật khẩu không chính xác.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                TempData["Error"] = $"Lỗi! {errorMsg}.";
                return View();
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [Authorize]
        public IActionResult Denied()
        {
            return View();
        }
    }
}
