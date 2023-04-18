using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace caa_mis.Data
{
    public class ApplicationDbInitializer
    {
        public static async void Seed(IApplicationBuilder applicationBuilder)
        {
            ApplicationDbContext context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                ////Delete the database if you need to apply a new Migration
                //context.Database.EnsureDeleted();
                //Create the database if it does not exist and apply the Migration
                context.Database.Migrate();

                //Create Roles
                var RoleManager = applicationBuilder.ApplicationServices.CreateScope()
                    .ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roleNames = { "Admin", "Supervisor" };

                IdentityResult roleResult;
                foreach (var roleName in roleNames)
                {
                    var roleExist = await RoleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                ////Create Branches
                //var branchRoleManager = applicationBuilder.ApplicationServices.CreateScope()
                //    .ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                //string[] branchNames = { "Grimsby", "Welland", "Thorold", "Niagara Falls", "St. Catharines" };

                //IdentityResult branchResult;
                //foreach (var branchName in branchNames)
                //{
                //    var branchExist = await branchRoleManager.RoleExistsAsync(branchName);
                //    if (!branchExist)
                //    {
                //        branchResult = await branchRoleManager.CreateAsync(new IdentityRole(branchName));
                //    }
                //}

                //Create Users
                var userManager = applicationBuilder.ApplicationServices.CreateScope()
                    .ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                if (userManager.FindByEmailAsync("admin@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "admin@outlook.com",
                        Email = "admin@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Admin").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("super@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "super@outlook.com",
                        Email = "super@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;

                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Supervisor").Wait();
                    }
                }
                if (userManager.FindByEmailAsync("user@outlook.com").Result == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "user@outlook.com",
                        Email = "user@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;
                    //Not in any role
                }
                //if (userManager.FindByEmailAsync("user@outlook.com").Result == null)
                //{
                //    IdentityUser user = new IdentityUser
                //    {
                //        UserName = "user@outlook.com",
                //        Email = "user@outlook.com"
                //    };

                //    IdentityResult result = userManager.CreateAsync(user, "Pa55w@rd").Result;
                //    //Not in any role
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
