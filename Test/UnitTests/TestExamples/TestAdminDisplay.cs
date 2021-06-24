﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using AuthPermissions;
using AuthPermissions.BulkLoadServices.Concrete;
using AuthPermissions.DataLayer.EfCode;
using AuthPermissions.SetupCode;
using Example4.MvcWebApp.IndividualAccounts.PermissionsCode;
using ExamplesCommonCode.DemoSetupCode;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Test.DiTestHelpers;
using Test.TestHelpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestExamples
{
    public class TestAdminDisplay
    {
        private readonly ITestOutputHelper _output;

        public TestAdminDisplay(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestExample4Setup()
        {
            //SETUP
            var services = new ServiceCollection();

            //ATTEMPT
            var context = await services.RegisterAuthPermissions<Example4Permissions>(options =>
                {
                    options.TenantType = TenantTypes.HierarchicalTenant;
                })
                .UsingInMemoryDatabase()
                .AddRolesPermissionsIfEmpty(Example4AppAuthSetupData.BulkLoadRolesWithPermissions)
                .AddTenantsIfEmpty(Example4AppAuthSetupData.BulkHierarchicalTenants)
                .AddUsersRolesIfEmptyWithUserIdLookup<StubIFindUserInfo>(Example4AppAuthSetupData.UsersRolesDefinition)
                .SetupForUnitTestingAsync();

            //VERIFY
            context.ChangeTracker.Clear();
            context.AuthUsers.Count().ShouldBeInRange(15,30);
            context.RoleToPermissions.Count().ShouldBeInRange(4, 15);
            context.UserToRoles.Count().ShouldBeInRange(20, 40);
            context.Tenants.Count().ShouldBeInRange(20, 40);
            context.AuthUsers.Count(x => x.TenantId != null).ShouldEqual(3);
        }


    }
}