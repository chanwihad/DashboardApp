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
            // ViewBag.data = response;
            // ViewBag.Current = model.CurrentPassword; 
            // ViewBag.New = model.NewPassword; 
            // ViewBag.Confirm = model.ConfirmNewPassword; 
            return Redirect("/");
            // return View(model);
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

    // [HttpPost]
    // public async Task<IActionResult> ForgetPassword(ResetPasswordModel model)
    // {
    //     if (checkLogin())
    //     {
    //         return Redirect("/");
    //     }

    //     var isForgot = await _authApiClient.ForgotPasswordAsync(model.Email);

    //     if (isForgot)
    //     {
    //         return Redirect("/");
    //     }

    //     ViewBag.ErrorMessage = "Invalid login credentials.";
    //     return View();
    // }

    [HttpPost]
    public async Task<IActionResult> SendVerificationCode(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Email is required.";
            return Redirect("/Auth/ForgetPassword");
        }

        // var user = await _authApiClient.GetUserByEmailAsync(email);
        // if (user == null)
        // {
        //     ViewBag.Error = "Email is not registered.";
        //     return View("ForgotPassword");
        // }

        // var verificationCode = _verificationCodeService.GenerateCode();
        // await _verificationCodeService.SaveCodeAsync(email, verificationCode);
        // await _emailService.SendEmailAsync(email, "Your Verification Code", $"Your code is: {verificationCode}");

        _generatedCode = new Random().Next(100000, 999999).ToString();
        TempData["Success"] = $"Verification code sent! (Code: {_generatedCode})";
        return Redirect("/Auth/ForgetPassword");
    }

    [HttpPost]
    public async Task<IActionResult> VerifyCode(string verificationCode)
    {
        // if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(verificationCode))
        // {
        //     ViewBag.Error = "Email and Verification Code are required.";
        //     return View("ForgotPassword");
        // }

        // var isValidCode = await _verificationCodeService.ValidateCodeAsync(email, verificationCode);
        // if (!isValidCode)
        // {
        //     ViewBag.Error = "Invalid verification code.";
        //     return View("ForgotPassword");
        // }

        // // Jika valid, redirect ke halaman untuk reset password
        // return RedirectToAction("ResetPassword", new { email });
        if (string.IsNullOrEmpty(verificationCode))
        {
            TempData["Error"] = "Verification code is required!";
            return Redirect("/Auth/ForgetPassword");
        }

        if (verificationCode != _generatedCode)
        {
            TempData["Error"] = "Invalid verification code!";
            return Redirect("/Auth/ForgetPassword");
        }

        TempData["Success"] = "Verification successful!";
        return Redirect("/Auth/Login");
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear(); 
        
        return RedirectToAction("Login");

    }
}
