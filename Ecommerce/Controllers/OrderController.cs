using Ecommerce.Data;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class OrderController : Controller
    {

        private readonly EcommerceContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderController(EcommerceContext db, IHttpContextAccessor httpContextAccessor,
           UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            var orders = (from order in _db.Orders.AsEnumerable()
                          join user in users on order.UserId equals user.Id
                          join status in _db.OrderStatus on order.OrderStatusId equals status.Id
                          select new { Username = user.UserName, StatusName = status.StatusName, CreateDate = order.CreateDate }).ToList();

            List<OrderList> orderList = new List<OrderList>(users.Count);
            foreach (var order in orders)
            {
                orderList.Add(new OrderList() { UserName = order.Username, StatusName = order.StatusName, CreateDate= order.CreateDate});
            }
            return View(orderList);
        }
    }
}
