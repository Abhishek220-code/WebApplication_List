using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication_List.Models
{
    public class TbEmp_Document
    {
        [Key]
        public Guid DocumentId { get; set; }

        [ForeignKey("TbEmpEmployee")]
        public Guid EmployeeId { get; set; }   
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public DateTime CreatedDate { get; set; }
        public TbEmp_Employee Employee { get; set; } // Navigation
    }

}
