using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class Product
    {
        [Key]
        [Display(Name = "編號")]
        public int Id { get; set; }
        [Required]
        [Display(Name = "書名")]
        public string? Title { get; set; }
        [Required]
        [Display(Name = "介紹")]
        public string? Description { get; set; }
        [Required]
        public string? ISBN { get; set; }
        [Required]
        [Display(Name = "作者")]
        public string? Author { get; set; }
        [Required]
        [Display(Name = "建議售價")]
        [Range(1, 1000)]
        public double? ListPrice { get; set; }
        [Required]
        [Display(Name = "售價(購買數量1-50)")]
        [Range(1, 1000)]
        public double? Price { get; set; }
        [Required]
        [Display(Name = "售價(購買數量50-100)")]
        [Range(1, 1000)]
        public double? Price50 { get; set; }
        [Required]
        [Display(Name = "售價(購買數量100以上)")]
        [Range(1, 1000)]
        public double? Price100 { get; set; }
        [Display(Name = "分類")]

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        //[Display(Name = "圖片上傳")]
        //public string? ImageUrl { get; set; }
        [ValidateNever]
        [Display(Name ="產品照片")]
        public List<ProductImage> ProductImages { get; set; }
    }
}
