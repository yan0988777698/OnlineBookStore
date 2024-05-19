using System.ComponentModel.DataAnnotations;

namespace BulkyWebRazor_Temp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "字數上限為100")]
        public string Name { get; set; }
        [Range(1, 100, ErrorMessage = "請輸入1-100之間的數字")]
        public int DisplayOrder { get; set; }

    }
}
