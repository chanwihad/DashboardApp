using Microsoft.AspNetCore.Mvc;
using DashboardApp.Services;
using DashboardApp.Models;
using Microsoft.AspNetCore.Http;

namespace DashboardApp.Controllers
{
    public class UserController : Controller
{
    private readonly UserApiClient _userApiClient;
    private readonly RoleApiClient _roleApiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PermissionHelper _permissionHelper;

    public UserController(UserApiClient userApiClient, RoleApiClient roleApiClient, IHttpContextAccessor httpContextAccessor, PermissionHelper permissionHelper)
    {
        _userApiClient = userApiClient;
        _roleApiClient = roleApiClient;
        _httpContextAccessor = httpContextAccessor;
        _permissionHelper = permissionHelper;
    }

    public async Task<IActionResult> Index(string searchQuery = "")
    {
        if (!_permissionHelper.CheckLogin())
        {
            return Redirect("/Auth/Login");
        }

        if (!_permissionHelper.HasAccess("CanView", "api/user"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
        }
        
        var users = await _userApiClient.GetUsers(searchQuery);
        return View(users);
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!_permissionHelper.CheckLogin())
        {
            return Redirect("/Auth/Login");
        }

        if (!_permissionHelper.HasAccess("CanCreate", "api/user"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
        }
            
        var roles = await _roleApiClient.GetRoles();

        ViewBag.Roles = roles;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (!_permissionHelper.CheckLogin())
        {
            return Redirect("/Auth/Login");
        }

        if (!_permissionHelper.HasAccess("CanCreate", "api/user"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
        }

        try
        {
            await _userApiClient.CreateUser(model);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Failed create new User.";
            return View("Error");
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!_permissionHelper.CheckLogin())
        {
            return Redirect("/Auth/Login");
        }

        if (!_permissionHelper.HasAccess("CanUpdate", "api/user"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
        }
        
        //  if (ModelState.IsValid)
        // {
            try
            {
                var user = await _userApiClient.GetUser(id);
            
                if (user == null)
                {
                    return NotFound();
                }

                ViewBag.Roles = await _roleApiClient.GetRoles(); 
                return View(user);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed show edit user.";
                return View("Error");
            }
        // }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, User model)
    {
        if (!_permissionHelper.CheckLogin())
        {
            return Redirect("/Auth/Login");
        }

        if (!_permissionHelper.HasAccess("CanUpdate", "api/user"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
        }

        try
        {
            if(model.Password == null)
                model.Password = "@#%empty021&^";

            await _userApiClient.UpdateUser(id, model);
            return RedirectToAction("Index");
            // return View(model);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Failed create new User.";
            return View("Error");
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!_permissionHelper.CheckLogin())
        {
            return Redirect("/Auth/Login");
        }

        if (!_permissionHelper.HasAccess("CanDelete", "api/user"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
        }

        try
        {
            await _userApiClient.DeleteUser(id);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Failed delete User.";
            return View("Error");
        }
    }
}

}
