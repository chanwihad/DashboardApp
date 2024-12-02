using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DashboardApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using DashboardApp.Models;
using DashboardApp.Services;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly AuthApiClient _authApiClient;

    public AuthController(IHttpClientFactory httpClientFactory, HttpClient httpClient, IConfiguration configuration, AuthApiClient authApiClient)
    {
        _httpClientFactory = httpClientFactory;
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);
        _authApiClient = authApiClient;
    }

    private bool checkLogin()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
    }

    public IActionResult Register()
    {
        if (checkLogin())
        {
            return Redirect("/");
        }

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (checkLogin())
        {
            return Redirect("/");
        }

        var isRegistered = await _authApiClient.RegisterAsync(model.Username, model.FullName, model.Email, model.Password);

        if (isRegistered)
        {
            return RedirectToAction("Login");
        }

        ViewBag.ErrorMessage = "Invalid register credentials.";
        return View();
    }

    public IActionResult Login() 
    {
        if (checkLogin())
        {
            return Redirect("/");
        }

        return View();
    } 

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (checkLogin())
        {
            return Redirect("/");
        }
        
        var isLoggedIn = await _authApiClient.LoginAsync(model.Username, model.Password);

        if (isLoggedIn)
        {
            return Redirect("/");
        }

        ViewBag.ErrorMessage = "Invalid login credentials.";
        return View();
    }

    public IActionResult ChangePassword() 
    {
        if (!checkLogin())
        {
            return Redirect("/");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        if (!checkLogin())
        {
            return Redirect("/");
        }

        var isChanged = await _authApiClient.ChangePasswordAsync(model.CurrentPassword, model.NewPassword);

        if (isChanged)
        {
            return Redirect("/");
        }

        ViewBag.ErrorMessage = "Invalid change passord credentials.";
        return View();
    }

    public IActionResult ForgetPassword() 
    {
        if (checkLogin())
        {
            return Redirect("/");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgetPassword(ResetPasswordModel model)
    {
        if (checkLogin())
        {
            return Redirect("/");
        }

        var isForgot = await _authApiClient.ForgotPasswordAsync(model.Email);

        if (isForgot)
        {
            return Redirect("/");
        }

        ViewBag.ErrorMessage = "Invalid login credentials.";
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear(); 
        
        return RedirectToAction("Login");

    }
}
