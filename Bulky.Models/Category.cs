using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "名稱")]
        [MaxLength(100, ErrorMessage = "字數上限為100")]
        public string? Name { get; set; }
        [Range(1, 100, ErrorMessage = "請輸入1-100之間的數字")]
        [Display(Name = "顯示順序")]
        public int DisplayOrder { get; set; }

    }
}
