using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly MyDBContext _db;

        public Category category { get; set; }

        public EditModel(MyDBContext db)
        {
            _db = db;
        }
        public void OnGet(int? id)
        {
            if (id != 0 && id != null)
                category = _db.Categories.Find(id);
        }
        public IActionResult OnPost(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(category);
                _db.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToPage("List");
            }
            return Page();
        }
    }
}
