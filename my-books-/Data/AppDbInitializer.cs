using Microsoft.AspNetCore.Identity;
using my_books.Data.Models;
using my_books.Data.ViewModels.Authentication;

namespace my_books.Data
{
    public class AppDbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

                if (!context.Books.Any())
                {
                    context.Books.AddRange(new List<Book>()
                    {
                        new Book()
                        {
                            Title = "The Hobbit",
                            Description = "A fantasy novel about Bilbo Baggins' adventure.",
                            IsRead = true,
                            DateRead = DateTime.Parse("2023-05-10"),
                            rate = 5,
                            Genre = "Fantasy",
                            CoverUrl = "https://example.com/hobbit.jpg",
                            DateAdded = DateTime.Parse("2023-01-01"),
                        },
                        new Book()
                        {
                            Title = "1984",
                            Description = "A dystopian novel about totalitarian surveillance.",
                            IsRead = false,
                            Genre = "Dystopian",
                            CoverUrl = "https://example.com/1984.jpg",
                            DateAdded = DateTime.Parse("2023-02-15"),
                        }
                    });
                    context.SaveChanges();
                }
            }
        }
        public static async Task SeedRoles(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

                if (!await roleManager.RoleExistsAsync(UserRoles.Publisher))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Publisher));

                if (!await roleManager.RoleExistsAsync(UserRoles.Author))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Author));

                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            }
        }
    }
}