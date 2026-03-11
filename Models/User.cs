using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_List.Models;

public partial class Users
{
    [Key]
    public Guid User_ID { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
}

