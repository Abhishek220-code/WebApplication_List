using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication_List.Models
{
    public class EmployeeVM
    {
        public TbEmp_Employee Employee { get; set; } = new();

        public IFormFile? DocumentFile { get; set; }

        public string? DocumentName { get; set; }

        public List<SelectListItem> Designations { get; set; } = new();
    }



}
