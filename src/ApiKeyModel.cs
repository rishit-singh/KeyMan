using System;
using System.Collections.Generic;

namespace KeyMan.Models;

public partial class ApiKeyModel 
{
    public string Key { get; set; } = null!;

    public string Userid { get; set; } = null!;

    public string? Permissions { get; set; }

    public string Creationtime { get; set; } = null!;

    public string? Expirytime { get; set; }

    public bool Islimitless { get; set; }
}
