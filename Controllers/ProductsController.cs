using DashboardApp.Models;
using DashboardApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace DashboardApp.Controllers
{    
    public class ProductsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ProductApiClient _productApiClient;
        private readonly PermissionHelper _permissionHelper;

        public ProductsController(HttpClient httpClient, ProductApiClient productApiClient, PermissionHelper permissionHelper)
        {
            _httpClient = httpClient;
            _productApiClient = productApiClient;
            _permissionHelper = permissionHelper;
        }
        

        public async Task<IActionResult> Index()
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanView", "api/product"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            try
            {
                var products = await _productApiClient.GetProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Gagal memuat daftar produk.";
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanCreate", "api/product"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }
            
            try
            {
                var product = await _productApiClient.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Gagal memuat detail produk.";
                return View("Error");
            }
        }

        public IActionResult Create()
        {
             if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanCreate", "api/product"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
             if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanCreate", "api/product"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productApiClient.CreateProductAsync(product);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Gagal menambahkan produk baru.";
                    return View("Error");
                }
            }
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanUpdate", "api/product"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            try
            {
                var product = await _productApiClient.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Gagal memuat data produk untuk diedit.";
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanUpdate", "api/product"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productApiClient.UpdateProductAsync(id, product);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Gagal memperbarui produk.";
                    return View("Error");
                }
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanDelete", "api/product"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }

            try
            {
                var product = await _productApiClient.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Gagal memuat data produk untuk dihapus.";
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
           if (!_permissionHelper.CheckLogin())
            {
                return Redirect("/Auth/Login");
            }

            if (!_permissionHelper.HasAccess("CanDelete", "api/product"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Access denied");
            }
            
            try
            {
                await _productApiClient.DeleteProductAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Gagal menghapus produk.";
                return View("Error");
            }
        }
    }
}