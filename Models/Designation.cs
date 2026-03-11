using System.ComponentModel.DataAnnotations;

namespace WebApplication_List.Models
{
    public class TbEmp_Designation
    {
        [Key]
        public Guid Designation_Id { get; set; }
        public required string Designation_Name { get; set; }
        public int? Priority { get; set; }
        public int Inactive { get; set; }

        //public ICollection<TbEmp_Employee> Employees { get; set; }
    }

}
