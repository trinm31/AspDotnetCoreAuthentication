using System.Linq;
using AuthenticationAspDotnetCore.Data;
using AuthenticationAspDotnetCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAspDotnetCore.Controllers
{
    //[Authorize(Roles = SD.Staff-Role)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // [Authorize(Roles = "Admin,Staff")]
        public IActionResult Index()
        {
            // get all data and store to list
            var listAllData = _db.Categories.ToList();
            return View(listAllData);
        }


        // [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Upsert(int? id) // allow null diền vào cũng dc mà ko cũng dc 
        {
            // create
            if (id == null)
            {
                return View(new Category());
            }

            // update
            var categories = _db.Categories.Find(id); // xuống db tìm lại category cũ theo id và truyền về view 
            return View(categories);
        }


        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public IActionResult Upsert(Category category) // nhận vào object category từ view
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    // create
                    _db.Categories.Add(category);
                }
                else
                {
                    // update
                    _db.Categories.Update(category);
                }

                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            // nếu những property không thỏa mãn yêu cầu input thì trả về lại view obj cũ kèm vs lại lỗi
            return View(category);
        }


        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            // find category
            var categoryNeedToDelete = _db.Categories.Find(id);
            // remove and save change
            _db.Categories.Remove(categoryNeedToDelete);
            _db.SaveChanges();
            // redirect to action index
            return RedirectToAction(nameof(Index));
        }
    }
}