
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApplication_List.Models;


namespace WebApplication_List.Controllers
{
    // [ApiController]

    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }


        //[HttpGet("signup")]
        public IActionResult Signup()
        {
            return View();
        }
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("List", "User");
            }

            return View();
        }


        [Authorize]
        public IActionResult List()
        {
            var vm = new EmployeeVM
            {
                Employee = new TbEmp_Employee(),
                Designations = _context.TbEmp_Designation
                    .Select(d => new SelectListItem
                    {
                        Value = d.Designation_Id.ToString(),
                        Text = d.Designation_Name
                    })
                    .ToList()
            };

            return View(vm);
        }



        public IActionResult Logout()
        {
            Response.Cookies.Delete("JWTToken");

            return RedirectToAction("Login");
        }



    }
}


