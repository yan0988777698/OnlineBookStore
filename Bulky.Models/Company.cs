using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name ="公司名稱")]
        [MaxLength(100, ErrorMessage = "字數上限為100")]
        public string Name { get; set; }
        [Display(Name = "詳細地址")]
        public string StreetAddress { get; set; }
        [Display(Name = "鄉鎮區")]
        public string Region { get; set; }
        [Display(Name = "縣市")]
        public string City { get; set; }
        [Display(Name = "電話")]
        public string PhoneNumber { get; set; }


    }
}
