﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUserClaim")]
public sealed class ApplicationUserClaim : IdentityUserClaim<string>
{
}