using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM _OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            _OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(_OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            OrderHeader orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == _OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = _OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = _OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = _OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.Region = _OrderVM.OrderHeader.Region;
            orderHeaderFromDb.City = _OrderVM.OrderHeader.City;
            orderHeaderFromDb.PostalCode = _OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(_OrderVM.OrderHeader.Carrier))
                orderHeaderFromDb.Carrier = _OrderVM.OrderHeader.Carrier;
            if (!string.IsNullOrEmpty(_OrderVM.OrderHeader.TrackingNumber))
                orderHeaderFromDb.TrackingNumber = _OrderVM.OrderHeader.TrackingNumber;
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "訂單修改成功";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(_OrderVM.OrderHeader.Id, SD.OrderStatusProcessing);
            _unitOfWork.Save();
            TempData["Success"] = "訂單修改成功";
            return RedirectToAction(nameof(Details), new { orderId = _OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == _OrderVM.OrderHeader.Id);
            orderHeaderFromDb.TrackingNumber = _OrderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = _OrderVM.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = SD.OrderStatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApprovedForDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "訂單已出貨";
            return RedirectToAction(nameof(Details), new { orderId = _OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == _OrderVM.OrderHeader.Id);
            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.OrderStatusCancelled, SD.OrderStatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.OrderStatusCancelled, SD.OrderStatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "訂單取消成功";
            return RedirectToAction(nameof(Details), new { orderId = _OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult PayNow()
        {
            _OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == _OrderVM.OrderHeader.Id,
                includeProperties: "ApplicationUser");
            _OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == _OrderVM.OrderHeader.Id,
                includeProperties: "Product");

            var domain = "https://localhost:7120/";
            //一般帳戶導向結帳畫面
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderHeaderId={_OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"Admin/Order/Details?orderHeaderId={_OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in _OrderVM.OrderDetail)
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

            _unitOfWork.OrderHeader.UpdateStripePaymentId(_OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            //重新導向
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApprovedForDelayedPayment)
            {
                //公司訂單
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            
            return View(orderHeaderId);
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
                string userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeaders = _unitOfWork.OrderHeader.GetAll(x => x.ApplicationUserId == userId,
                    includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "inprocess":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.OrderStatusProcessing);
                    break;
                case "pending":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.OrderStatusPanding);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.OrderStatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.OrderStatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
