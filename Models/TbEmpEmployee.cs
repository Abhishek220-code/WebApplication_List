using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_List.Models;

public partial class TbEmp_Employee
{
    [Key]
    public Guid EmployeeID { get; set; } 
    public string EmployeeName { get; set; } = null!;

    [ForeignKey("Designation")]
    public Guid Emp_DesignationId { get; set; }
    public string? Emp_Address { get; set; }
    public Guid Created_By { get; set; }
    public DateTime Created_Time { get; set; }
    public Guid? Modified_By { get; set; }
    public DateTime? Modified_Time { get; set; } 
    public short? GenderId { get; set; } 
    public DateOnly? DateofJoining { get; set; } 
    public int? Status { get; set; } 
    public  TbEmp_Designation Designation { get; set; }
    public ICollection<TbEmp_Document> Documents { get; set; }
}

