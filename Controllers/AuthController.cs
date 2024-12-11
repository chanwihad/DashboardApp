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
    private static string? _generatedCode;

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
        
        return Redirect("/");
        // return View();
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

        if(model.NewPassword == model.CurrentPassword)
        {
            ViewBag.Error = "New password and old password cannot be identical.";
            return View(model);
        }

        if(model.NewPassword != model.ConfirmNewPassword)
        {
            ViewBag.Error = "Password do not match. Please try again";
            return View(model);
        }

        try
        {
            var response = await _authApiClient.ChangePasswordAsync(model);
            return Redirect("/");
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Failed change new password.";
            return View(model);
        }
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
    public async Task<IActionResult> SendVerificationCode(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Email is required.";
            return Redirect("/Auth/ForgetPassword");
        }

        var response = await _authApiClient.SendVerificationCode(email);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<VerificationRequest>();
            if (result != null)
            {
                string verificationCode = result.Code;
                string verificationMail = result.Email;
                TempData["Success"] = $"Verification code sent! (Code: {verificationCode})";
                TempData["Mail"] = $"{email}";
            }
            else
            {
                TempData["Error"] = "Failed to parse verification code.";
            }
        }
        else
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
        }


        return Redirect("/Auth/ForgetPassword");
    }

    [HttpPost]
    public async Task<IActionResult> VerifyCode(VerificationRequest model)
    {
        if (string.IsNullOrEmpty(model.Code))
        {
            TempData["Error"] = "Verification code is required!";
            return Redirect("/Auth/ForgetPassword");
        }

        var response = await _authApiClient.VerifyCode(model);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<VerificationValidRequest>();
            TempData["Valid"] = "Verification successful!";
            TempData["Success"] = "Verification successful"; 
            TempData["ValidEmail"] = result.Email;
            return Redirect("/Auth/ForgetPassword");    
        }

        return Redirect("/Auth/Login");
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(NewPassword model)
    {
        if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
        {
            TempData["Error"] = "Password is required!";
            TempData["Valid"] =  "Verification retry!";
            TempData["ValidEmail"] = model.Email;
            return Redirect("/Auth/ForgetPassword");
        }

        if(model.Password != model.ConfirmPassword)
        {
            TempData["Error"] = "Confirm Password must identical!";
            TempData["Valid"] =  "Verification retry!";
            TempData["ValidEmail"] = model.Email;
            return Redirect("/Auth/ForgetPassword");
        }

        var response = await _authApiClient.ResetPassword(model);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Reset Password Success!";
            return Redirect("/Auth/Login");
        }
        TempData["Error"] = "Reset Password Failed. Try again!";
        return Redirect("/Auth/ForgetPassword");

    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear(); 
        
        return RedirectToAction("Login");

    }
}
