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
                OrderHeader=new OrderHeader
                {
                    OrderTotal = (double)sum
                }
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
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            cartFromDb.Count++;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count--;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
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
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
            shoppingCartVM = new()
            {
                ShoppingCartList = shoppingCartByUserId,
                OrderHeader = new OrderHeader
                {
                    ApplicationUser = user,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    StreetAddress = user.StreetAddress,
                    Region = user.Region,
                    City = user.City,
                    PostalCode = user.PostalCode,
                    OrderTotal = (double)sum
                }
            };
            return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
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
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
            shoppingCartVM = new()
            {
                ShoppingCartList = shoppingCartByUserId,
                OrderHeader = new OrderHeader
                {
                    ApplicationUser = user,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    StreetAddress = user.StreetAddress,
                    Region = user.Region,
                    City = user.City,
                    PostalCode = user.PostalCode,
                    OrderTotal = (double)sum
                }
            };
            return View(shoppingCartVM);
        }
    }
}
