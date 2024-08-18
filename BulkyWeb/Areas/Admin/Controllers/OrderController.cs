using Bulky.DataAccess.Repo.IRepo;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Climate;
using System.Diagnostics;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM _OrderVM {  get; set; }
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
        [Authorize(Roles = SD.Role_Admin +","+ SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            OrderHeader orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == _OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = _OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = _OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = _OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.Region = _OrderVM.OrderHeader.Region;
            orderHeaderFromDb.City = _OrderVM.OrderHeader.City;
            orderHeaderFromDb.PostalCode = _OrderVM.OrderHeader.PostalCode;
            if(!string.IsNullOrEmpty(_OrderVM.OrderHeader.Carrier))
                orderHeaderFromDb.Carrier = _OrderVM.OrderHeader.Carrier;
            if (!string.IsNullOrEmpty(_OrderVM.OrderHeader.TrackingNumber))
                orderHeaderFromDb.TrackingNumber = _OrderVM.OrderHeader.TrackingNumber;
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "訂單修改成功";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id});
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
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
