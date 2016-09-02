﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ITI.PrimarySchool.DAL;
using ITI.PrimarySchool.WebApp.Authentication;
using ITI.PrimarySchool.WebApp.Models.AccountViewModels;
using ITI.PrimarySchool.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.PrimarySchool.WebApp.Controllers
{
    public class AccountController : Controller
    {
        readonly UserService _userService;

        public AccountController( UserService userService )
        {
            _userService = userService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login( string returnUrl = null )
        {
            ViewData[ "ReturnUrl" ] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login( LoginViewModel model, string returnUrl = null )
        {
            if( ModelState.IsValid )
            {
                User user = _userService.FindUser( model.Email, model.Password );
                if( user == null )
                {
                    ModelState.AddModelError( string.Empty, "Invalid login attempt." );
                    ViewData[ "ReturnUrl" ] = returnUrl;
                    return View( model );
                }
                List<Claim> claims = new List<Claim>
                {
                    new Claim( ClaimTypes.Email, model.Email, ClaimValueTypes.String ),
                    new Claim( ClaimTypes.NameIdentifier, user.UserId.ToString(), ClaimValueTypes.String )
                };
                ClaimsIdentity identity = new ClaimsIdentity( claims, "Cookies", ClaimTypes.Email, string.Empty );
                ClaimsPrincipal principal = new ClaimsPrincipal( identity );
                await HttpContext.Authentication.SignInAsync( CookieAuthentication.AuthenticationScheme, principal );
                return RedirectToLocal( returnUrl );
            }

            return View( model );
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register( string returnUrl = null )
        {
            ViewData[ "ReturnUrl" ] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register( RegisterViewModel model, string returnUrl = null )
        {
            if( ModelState.IsValid )
            {
                if( !_userService.CreateUser( model.Email, model.Password ) )
                {
                    ModelState.AddModelError( string.Empty, "An account with this email already exists." );
                    return View( model );
                }
                return RedirectToLocal( returnUrl );
            }

            return View( model );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.Authentication.SignOutAsync( CookieAuthentication.AuthenticationScheme );
            return RedirectToAction( "Index", "Home" );
        }

        IActionResult RedirectToLocal( string returnUrl )
        {
            if( Url.IsLocalUrl( returnUrl ) )
            {
                return Redirect( returnUrl );
            }
            else
            {
                return RedirectToAction( nameof( HomeController.SinglePageApp ), "Home" );
            }
        }
    }
}
