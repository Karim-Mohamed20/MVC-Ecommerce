using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly EcommerceContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(EcommerceContext db, IHttpContextAccessor httpContextAccessor,
           UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

        private Cart GetCart(string userId)
        {
            var cart = _db.Carts.FirstOrDefault(x => x.UserId == userId);
            return cart;
        }

        private int GetCartItemCount(string userId = "")
        {
            if (!string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = (from cart in _db.Carts
                              join cartDetail in _db.CartDetails
                              on cart.Id equals cartDetail.CartId
                              select new { cartDetail.Id }
                        ).ToList();
            return data.Count;
        }

        public IActionResult GetTotalItemInCart ()
        {
            int cartItem = GetCartItemCount();
            return Ok(cartItem);
        }

        public IActionResult AddItem(int productId, int qty = 1, int redirect = 0)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");

                var principal = _httpContextAccessor.HttpContext.User;
                //if (principal.IsInRole("Admin"))
                //{
                //    throw new Exception("Wrong User");
                //}
                var cart = GetCart(userId);
                if (cart is null)
                {
                    cart = new Cart
                    {
                        UserId = userId
                    };
                    _db.Carts.Add(cart);
                }
                _db.SaveChanges();

                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.CartId == cart.Id && a.ProductId == productId);
                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var product = _db.Products.Find(productId);
                    cartItem = new CartDetail
                    {
                        ProductId = productId,
                        CartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = product.UnitPrice,
                        TotalPrice = qty * product.UnitPrice
                    };
                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
            }
            var cartItemCount = GetCartItemCount(userId);
            if (redirect == 0)
                return Ok(cartItemCount);
            return RedirectToAction("GetUserCart");
        }

        public IActionResult RemoveItem(int productId)
        {
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");
                var cart = GetCart(userId);
                if (cart is null)
                    throw new Exception("Invalid cart");
                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.CartId == cart.Id && a.ProductId == productId);
                if (cartItem is null)
                    throw new Exception("Not items in cart");
                else if (cartItem.Quantity == 1)
                    _db.CartDetails.Remove(cartItem);
                else
                    cartItem.Quantity = cartItem.Quantity - 1;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            var cartItemCount = GetCartItemCount(userId);
            return RedirectToAction("GetUserCart");
        }

        public IActionResult GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new Exception("Invalid userid");
            var cart = _db.Carts
                                  .Include(a => a.CartDetails)
                                  .ThenInclude(a => a.Product)
                                  .Where(a => a.UserId == userId).FirstOrDefault();
            return View(cart);
        }

        public IActionResult Checkout()
        {
            using var transaction = _db.Database.BeginTransaction();
            bool isCheckedOut;
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var cart = GetCart(userId);
                if (cart is null)
                    throw new Exception("Invalid cart");
                var cartDetail = _db.CartDetails
                                    .Where(a => a.CartId == cart.Id).ToList();
                if (cartDetail.Count == 0)
                    throw new Exception("Cart is empty");
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    OrderStatusId = 1
                };
                _db.Orders.Add(order);
                _db.SaveChanges();
                foreach (var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    };
                    _db.OrderDetails.Add(orderDetail);
                }
                _db.SaveChanges();

                _db.CartDetails.RemoveRange(cartDetail);
                _db.SaveChanges();
                transaction.Commit();

                isCheckedOut = true;
            }
            catch (Exception)
            {

                isCheckedOut = false;
            }

            if (!isCheckedOut)
                throw new Exception("Something happen in server side");
            return RedirectToAction("Index", "Home");
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
