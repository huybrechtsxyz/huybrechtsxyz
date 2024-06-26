﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("ApplicationUserLogin")]
[PrimaryKey(nameof(LoginProvider), nameof(ProviderKey))]
public sealed class ApplicationUserLogin : IdentityUserLogin<string>
{
}