using System.Collections.Generic;
using System.Linq;
using AuthenticationAspDotnetCore.Data;
using AuthenticationAspDotnetCore.Models;
using AuthenticationAspDotnetCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAspDotnetCore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        
        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }
        
        public IActionResult Index()
        {
            var productList = _db.Products.Include(x=> x.Category).ToList();
            return View(productList);
        }

        public IActionResult UpSert(int? id)
        {
            ProductVm productVm = new ProductVm();

            productVm.CategoriesList = categoriesSelectListItems(); 
            
            if (id == null)
            {
                productVm.Product = new Product();
                return View(productVm);
            }

            var productFromDb = _db.Products.Find(id);
            productVm.Product = productFromDb;
            return View(productVm);
        }

        [HttpPost]
        public IActionResult UpSert(ProductVm productVm)
        {
            if (ModelState.IsValid)
            {
                if (productVm.Product.Id == 0)
                {
                    _db.Products.Add(productVm.Product);
                }
                else
                {
                    _db.Products.Update(productVm.Product);
                }
                _db.SaveChanges();
            
                return RedirectToAction(nameof(Index));
            }

            productVm.CategoriesList = categoriesSelectListItems();
            
            return View(productVm);
        }

        [NonAction]
        private IEnumerable<SelectListItem> categoriesSelectListItems()
        {
            var categories = _db.Categories.ToList();
            var result = categories.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            return result;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var productNeedToDelete = _db.Products.Find(id);
            _db.Products.Remove(productNeedToDelete);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}