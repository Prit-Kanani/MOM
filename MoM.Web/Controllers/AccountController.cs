using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoM.Web.Models;

namespace MoM.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var response = await SendAuthRequestAsync("api/auth/login", new
            {
                userName = model.UserName,
                password = model.Password
            });

            if (!response.Success || response.Data is null)
            {
                ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Login failed.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            await SignInAsync(response.Data);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await SendAuthRequestAsync("api/auth/register", new
            {
                userName = model.UserName,
                password = model.Password
            });

            if (!response.Success || response.Data is null)
            {
                ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Registration failed.");
                return View(model);
            }

            await SignInAsync(response.Data);
            return RedirectToAction("Dashboard", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInAsync(AuthApiResponse authData)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, authData.UserId.ToString()),
                new(ClaimTypes.Name, authData.UserName),
                new("access_token", authData.Token),
                new("access_token_expires", authData.ExpiresAtUtc.ToString("O"))
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = authData.ExpiresAtUtc
                });
        }

        private async Task<(bool Success, AuthApiResponse? Data, string? ErrorMessage)> SendAuthRequestAsync(string path, object payload)
        {
            var client = _httpClientFactory.CreateClient("MomApi");
            using var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json")
            };

            using var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (false, null, string.IsNullOrWhiteSpace(content) ? "Authentication request failed." : content);
            }

            var data = JsonSerializer.Deserialize<AuthApiResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data is null)
            {
                return (false, null, "Authentication response was invalid.");
            }

            return (true, data, null);
        }
    }
}
