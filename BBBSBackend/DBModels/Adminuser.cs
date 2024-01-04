using System;
using System.Collections.Generic;

namespace BBBSBackend.DBModels;

public partial class AdminUser
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;
}
