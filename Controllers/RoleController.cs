using Microsoft.AspNetCore.Mvc;
using DashboardApp.Services;
using DashboardApp.Models;

namespace DashboardApp.Controllers
{
    public class RoleController : Controller
    {
        private readonly RoleApiClient _roleApiClient;
        private readonly MenuApiClient _menuApiClient;
        private readonly PermissionHelper _permissionHelper;

        public RoleController(RoleApiClient roleApiClient, MenuApiClient menuApiClient, PermissionHelper permissionHelper)
        {
            _roleApiClient = roleApiClient;
            _menuApiClient = menuApiClient;
            _permissionHelper = permissionHelper;
        }

        public async Task<IActionResult> Index()
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanView", "api/role"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            var roles = await _roleApiClient.GetRoles();
            if (roles == null)
            {
                throw new InvalidOperationException("Roles data is null.");
            }
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanCreate", "api/role"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            var menus = await _menuApiClient.GetMenus();

            ViewBag.Menus = menus;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleRequest model)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanCreate", "api/role"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            try
            {
                await _roleApiClient.CreateRole(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed created new role.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanUpdate", "api/role"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            var role = await _roleApiClient.GetRole(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = "Role not found.";
                return RedirectToAction("Index");
            }

            var menus = await _menuApiClient.GetMenus();

            ViewBag.Menus = menus;
            
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, RoleRequest model)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanUpdate", "api/role"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _roleApiClient.UpdateRole(id, model);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Failed create new Role";
                    return View("Error");
                }
            }
            ViewBag.Menus = await _menuApiClient.GetMenus(); 
            // ViewBag.ErrorMessage = model.Description;
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanDelete", "api/role"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            try
            {
                await _roleApiClient.DeleteRole(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed delete Role.";
                return View("Error");
            }
        }
    }
}
