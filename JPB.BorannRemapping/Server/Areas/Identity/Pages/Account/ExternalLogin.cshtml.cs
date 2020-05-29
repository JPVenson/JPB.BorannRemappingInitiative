﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JPB.BorannRemapping.Server.Data.AppContext;
using Microsoft.AspNetCore.Authorization;
using JPB.BorannRemapping.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace JPB.BorannRemapping.Server.Areas.Identity.Pages.Account
{
	[AllowAnonymous]
	public class ExternalLoginModel : PageModel
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IEmailSender _emailSender;
		private readonly AppDbContext _context;
		private readonly ILogger<ExternalLoginModel> _logger;

		public ExternalLoginModel(
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ILogger<ExternalLoginModel> logger,
			IEmailSender emailSender,
			AppDbContext context)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_roleManager = roleManager;
			_logger = logger;
			_emailSender = emailSender;
			_context = context;
		}

		[BindProperty]
		public InputModel Input { get; set; }

		public string LoginProvider { get; set; }

		public string ReturnUrl { get; set; }

		[TempData]
		public string ErrorMessage { get; set; }

		public class InputModel
		{
			[Required]
			[EmailAddress]
			public string Email { get; set; }

			[Required]
			public string CommanderName { get; set; }
		}

		public IActionResult OnGetAsync()
		{
			return RedirectToPage("./Login");
		}

		public IActionResult OnPost(string provider, string returnUrl = null)
		{
			// Request a redirect to the external login provider.
			var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
		{
			returnUrl = returnUrl ?? Url.Content("~/");
			if (remoteError != null)
			{
				ErrorMessage = $"Error from external provider: {remoteError}";
				return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
			}
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				ErrorMessage = "Error loading external login information.";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}
			
			// Sign in the user with this external login provider if the user already has a login.
			var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor : true);
			if (result.Succeeded)
			{
				_logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
				var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				var token = _context.FrontierOauthToken.FirstOrDefault(e => e.IdUser == user.Id);
				token.RefreshToken = info.Principal.FindFirstValue("frontier:refreshtoken");
				token.Token = info.Principal.FindFirstValue("frontier:token");
				token.TokenValidUntil = DateTimeOffset.UtcNow.AddSeconds(Convert.ToDouble(info.Principal.FindFirstValue("frontier:tokenvalid")));
				await _context.SaveChangesAsync();
				return LocalRedirect(returnUrl);
			}
			if (result.IsLockedOut)
			{
				return RedirectToPage("./Lockout");
			}
			else
			{
				// If the user does not have an account, then ask the user to create an account.
				ReturnUrl = returnUrl;
				LoginProvider = info.LoginProvider;
				if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
				{
					Input = new InputModel
					{
						Email = info.Principal.FindFirstValue(ClaimTypes.Email),
						CommanderName = info.Principal.FindFirstValue(FrontierClaims.CommanderName)
					};
				}
				return Page();
			}
		}

		public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
		{
			returnUrl = returnUrl ?? Url.Content("~/");
			// Get the information about the user from the external login provider
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				ErrorMessage = "Error loading external login information during confirmation.";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}

			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = Input.Email, 
					Email = Input.Email,
					CommanderName = info.Principal.FindFirstValue(FrontierClaims.CommanderName),
				};
				var result = await _userManager.CreateAsync(user);
				if (result.Succeeded)
				{
					result = await _userManager.AddLoginAsync(user, info);
					var addToRoleAsync = await _userManager.AddToRoleAsync(user, "User");
					if (result.Succeeded && addToRoleAsync.Succeeded)
					{
						_logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

						var userId = await _userManager.GetUserIdAsync(user);
						var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
						code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
						var callbackUrl = Url.Page(
							"/Account/ConfirmEmail",
							pageHandler: null,
							values: new { area = "Identity", userId = userId, code = code },
							protocol: Request.Scheme);
						await _context.FrontierOauthToken.AddAsync(new FrontierOauthToken()
						{
							IdUser = userId,
							RefreshToken = info.Principal.FindFirstValue("frontier:refreshtoken"),
							Token = info.Principal.FindFirstValue("frontier:token"),
							TokenValidUntil = DateTimeOffset.UtcNow.AddSeconds(Convert.ToDouble(info.Principal.FindFirstValue("frontier:tokenvalid"))),
						});
						await _context.SaveChangesAsync();

						await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
							$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

						// If account confirmation is required, we need to show the link if we don't have a real email sender
						if (_userManager.Options.SignIn.RequireConfirmedAccount)
						{
							return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
						}

						// Include the access token in the properties
						//var props = new AuthenticationProperties();
						//props.StoreTokens(info.AuthenticationTokens);
						//props.IsPersistent = true;
						//await _signInManager.SignInAsync(user, props);

						return LocalRedirect(returnUrl);
					}
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			LoginProvider = info.LoginProvider;
			ReturnUrl = returnUrl;
			return Page();
		}
	}
}
