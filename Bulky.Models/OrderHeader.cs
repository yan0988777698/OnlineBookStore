using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulky.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public double OrderTotal { get; set; }

        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateOnly PaymentDueDate { get; set; }
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }
        [Required(ErrorMessage = "連絡電話 必填")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "詳細地址 必填")]
        public string StreetAddress { get; set; }
        [Required(ErrorMessage = "鄉鎮區 必填")]
        public string Region { get; set; }
        [Required(ErrorMessage = "縣市 必填")]
        public string City { get; set; }
        [Required(ErrorMessage ="郵遞區號 必填")]
        public string PostalCode { get; set; }
        [Required(ErrorMessage = "姓名 必填")]
        public string Name { get; set; }
    }
}
