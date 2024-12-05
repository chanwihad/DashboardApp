using Microsoft.AspNetCore.Mvc;
using DashboardApp.Services;
using DashboardApp.Models;

namespace DashboardApp.Controllers
{
    public class MenuController : Controller
    {
        private readonly MenuApiClient _menuApiClient;
        private readonly PermissionHelper _permissionHelper;

        public MenuController(MenuApiClient menuApiClient, PermissionHelper permissionHelper)
        {
            _menuApiClient = menuApiClient;
            _permissionHelper = permissionHelper;
        }

        public async Task<IActionResult> Index(string searchQuery = "")
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanView", "api/menu"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            var menus = await _menuApiClient.GetMenus(searchQuery);
            if (menus == null)
            {
                throw new InvalidOperationException("Menu data is null.");
            }
            ViewData["Search"] = searchQuery;  
            return View(menus);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanCreate", "api/menu"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Menu model)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanCreate", "api/menu"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            try
            {
                await _menuApiClient.CreateMenu(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed created new menu.";
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

            if (!_permissionHelper.HasAccess("CanUpdate", "api/menu"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            var menu = await _menuApiClient.GetMenu(id);
            if (menu == null)
            {
                ViewBag.ErrorMessage = "Menu not found.";
                return RedirectToAction("Index");
            }

            return View(menu);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Menu model)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanUpdate", "api/menu"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _menuApiClient.UpdateMenu(id, model);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Failed create new Menu";
                    return View("Error");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanDelete", "api/menu"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            try
            {
                await _menuApiClient.DeleteMenu(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed delete Menu.";
                return View("Error");
            }
        }
    }
}
