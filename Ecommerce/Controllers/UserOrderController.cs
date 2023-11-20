using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class UserOrderController : Controller
    {
        private readonly EcommerceContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;


        public UserOrderController(EcommerceContext db,
            UserManager<IdentityUser> userManager,
             IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

        public IActionResult UserOrders()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not logged-in");
            var orders =  _db.Orders
                            .Include(x => x.OrderStatus)
                            .Include(x => x.OrderDetails)
                            .ThenInclude(x => x.Product)
                            .Where(a => a.UserId == userId)
                            .ToList();
            return View(orders);
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
