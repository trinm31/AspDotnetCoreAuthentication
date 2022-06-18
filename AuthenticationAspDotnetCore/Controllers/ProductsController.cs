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
            //1. Create product viewmodel
            ProductVm productVm = new ProductVm();

            //2. Set data for category lít
            productVm.CategoriesList = categoriesSelectListItems(); 
            
            //3. if create case id == null
            if (id == null)
            {
                // set product is new obj
                productVm.Product = new Product();
                
                return View(productVm);
            }
            
            // 4. Update case
            var productFromDb = _db.Products.Find(id);
            productVm.Product = productFromDb;
            return View(productVm);
        }

        [HttpPost]
        public IActionResult UpSert(ProductVm productVm)
        {
            // 1. Valid Case
            if (ModelState.IsValid)
            {
                // 1.1 create case
                if (productVm.Product.Id == 0)
                {
                    _db.Products.Add(productVm.Product);
                }
                else
                {
                    // 1.2 update case
                    _db.Products.Update(productVm.Product);
                }
                // 1.3 save
                _db.SaveChanges();
                // 1.4 đưa về trang index               
                return RedirectToAction(nameof(Index));
            }
            // 2. Not valid case
            // 2.1 cấp lại dữ liệu cho categoryList 
            productVm.CategoriesList = categoriesSelectListItems();
            // 2.2 trả về lại form upsert kèm vs thông tin cũ và lỗi message
            return View(productVm);
        }

        [NonAction]
        private IEnumerable<SelectListItem> categoriesSelectListItems()
        {
            // 1. Get list of categories
            var categories = _db.Categories.ToList();
            // 2. Cứ mỗi thằng category thì e sẽ tạo ra 1 thằng selectlistitem
            var result = categories.Select(x => new SelectListItem()
            {
                // hiện thị ra cho người dùng
                Text = x.Name,
                // giá trị của sellect item
                Value = x.Id.ToString()
            });

            return result;
        }

        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var productNeedToDelete = _db.Products.Find(id);
            _db.Products.Remove(productNeedToDelete);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}