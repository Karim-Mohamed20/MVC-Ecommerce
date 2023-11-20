using Ecommerce.Data;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class CustomerController : Controller
    {
        private readonly EcommerceContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerController(EcommerceContext db, IHttpContextAccessor httpContextAccessor,
           UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            List<UserList> userList = new List<UserList>(users.Count);
            foreach (var user in users)
            {
                userList.Add(new UserList() { UserName = user.UserName, PhoneNumber = user.PhoneNumber, Email = user.Email});
            }
            return View(userList);
        }
    }
}
