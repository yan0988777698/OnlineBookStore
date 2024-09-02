using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using Stripe.BillingPortal;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        [BindProperty]
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
                OrderHeader = new OrderHeader
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
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(x => x.UserId == cartFromDb.UserId).Count() - 1);
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
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(x => x.UserId == cartFromDb.UserId).Count() - 1);
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
        public IActionResult SummaryPOST(ShoppingCartVM shoppingCartVM)
        {
            double? sum = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == userId, includeProperties: "Product");
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);


            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                item.Price = GetPriceBasedOnQuantity(item);
                sum += item.Price * item.Count;
            }
            shoppingCartVM.OrderHeader.OrderTotal = (double)sum;

            if (shoppingCartVM.OrderHeader.ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //一般使用者，先付款後出貨
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPanding;
            }
            else
            {
                //公司使用者，先出貨並在出貨後30天內付款
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApprovedForDelayedPayment;
                shoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail order = new()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = (double)item.Price,
                    Count = item.Count,
                };
                _unitOfWork.OrderDetail.Add(order);
            }
            _unitOfWork.Save();

            if (shoppingCartVM.OrderHeader.ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7120/";
                //一般帳戶導向結帳畫面
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/ShoppingCart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "Customer/ShoppingCart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach (var item in shoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "TWD",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStripePaymentId(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                //重新導向
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            //公司用戶導向確認畫面
            return RedirectToAction(nameof(OrderConfirmation), new { id = shoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusApprovedForDelayedPayment)
            {
                //一般消費者訂單
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.OrderStatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            //清除購物車
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(x => x.UserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }
    }
}
