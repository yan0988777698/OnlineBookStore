using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private ShoppingCartVM shoppingCartVM { get; set; }
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize]
        public IActionResult Index()
        {
            double? sum = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            IEnumerable<ShoppingCart> shoppingCartByUserId = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId, includeProperties: "Product");
            foreach (var item in shoppingCartByUserId)
            {
                item.Price = GetPriceBasedOnQuantity(item);
                sum += item.Price * item.Count;
            }
            shoppingCartVM = new()
            {
                ShoppingCartList = shoppingCartByUserId,
                OrderTotal = (double)sum,
            };
            return View(shoppingCartVM);
        }

        private double? GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
                return shoppingCart.Product.Price;

            if (shoppingCart.Count <= 100)
                return shoppingCart.Product.Price50;

            return shoppingCart.Product.Price100;

        }
    }
}
