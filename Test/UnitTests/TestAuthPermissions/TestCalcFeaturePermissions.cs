﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using AuthPermissions.DataLayer.Classes;
using AuthPermissions.DataLayer.EfCode;
using AuthPermissions.PermissionsCode;
using Test.TestHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestAuthPermissions
{
    public class TestCalcFeaturePermissions
    {
        [Fact]
        public async Task TestCalcAllowedPermissionsSimple()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AuthPermissionsDbContext>();
            using var context = new AuthPermissionsDbContext(options);
            context.Database.EnsureCreated();

            var rolePer1 = RoleToPermissions.CreateRoleWithPermissions("Role1", null, 
                $"{(char) 1}{(char) 3}", context).Result;
            var rolePer2 = RoleToPermissions.CreateRoleWithPermissions("Role2", null,
                $"{(char)2}{(char)3}", context).Result;
            context.AddRange(rolePer1, rolePer2);
            var user = new AuthUser("User1", "User1@g.com", null, new[] {rolePer1});
            context.Add(user);
            context.SaveChanges();

            context.ChangeTracker.Clear();

            var service = new CalcAllowedPermissions(context);

            //ATTEMPT
            var permissions = await service.CalcPermissionsForUserAsync("User1");

            //VERIFY
            permissions.ShouldEqual($"{(char)1}{(char)3}");
        }

        [Fact]
        public async Task TestCalcAllowedPermissionsOverlap()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AuthPermissionsDbContext>();
            using var context = new AuthPermissionsDbContext(options);
            context.Database.EnsureCreated();

            var rolePer1 = RoleToPermissions.CreateRoleWithPermissions("Role1", null,
                $"{(char)1}{(char)3}", context).Result;
            var rolePer2 = RoleToPermissions.CreateRoleWithPermissions("Role2", null,
                $"{(char)2}{(char)3}", context).Result;
            context.AddRange(rolePer1, rolePer2);
            var user = new AuthUser("User1", "User1@g.com", null, new[] { rolePer1, rolePer2 });
            context.Add(user);
            context.SaveChanges();

            context.ChangeTracker.Clear();

            var service = new CalcAllowedPermissions(context);

            //ATTEMPT
            var permissions = await service.CalcPermissionsForUserAsync("User1");

            //VERIFY
            new string(permissions.OrderBy(x => x).ToArray()).ShouldEqual($"{(char)1}{(char)2}{(char)3}");
        }
    }
}