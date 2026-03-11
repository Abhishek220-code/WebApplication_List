using System.ComponentModel.DataAnnotations;

namespace WebApplication_List.Models
{
    public class SignupVM
    {

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

}

