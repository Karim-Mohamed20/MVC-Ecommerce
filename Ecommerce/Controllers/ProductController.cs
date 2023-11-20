using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly EcommerceContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductController(EcommerceContext db, IHttpContextAccessor httpContextAccessor,
           UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            List<Product> products = _db.Products.ToList();
            return View(products);
        }
        public IActionResult Detail(int id)
        {
            Product? product = _db.Products.FirstOrDefault(i => i.Id == id);
            return View(product);
        }
        public IActionResult New()
        {
            return View();
        }
        public IActionResult SaveNew(Product product)
        {
                _db.Products.Add(product);
                _db.SaveChanges();
                return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            return View(_db.Products.Find(id));
        }

        [HttpPost]
        public IActionResult Edit(Product product, [FromRoute] int id)
        {

                Product oldprod = _db.Products.FirstOrDefault(i => i.Id == id);
                oldprod.Name = product.Name;
                oldprod.UnitPrice = product.UnitPrice;
                oldprod.ImgUrl = product.ImgUrl;
                _db.SaveChanges();
                return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {

            return View(_db.Products.Find(id));
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            return View(_db.Products.Find(id));
        }
        [HttpPost]
        public IActionResult Delete(Product product, [FromRoute] int id)
        {
            var prod = _db.Products.Find(id);
            _db.Remove(prod);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
