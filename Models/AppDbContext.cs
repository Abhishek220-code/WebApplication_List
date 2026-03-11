using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_List.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<TbEmp_Employee> TbEmp_Employee { get; set; }

    public DbSet<TbEmp_Designation> TbEmp_Designation { get; set; }
    public DbSet<TbEmp_Document> TbEmp_Document { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       => optionsBuilder.UseSqlServer(
           "Server=DESKTOP-38P3UCK\\SQLEXPRESS;Database=Employee_09022026;Trusted_Connection=True;TrustServerCertificate=True;");


}
