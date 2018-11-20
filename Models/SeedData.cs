using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeBank.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new RecipeBankContext(
                serviceProvider.GetRequiredService<DbContextOptions<RecipeBankContext>>()))
            {
                // Look for any movies.
                if (context.RecipeItem.Count() > 0)
                {
                    return;   // DB has been seeded
                }

                context.RecipeItem.AddRange(
                    new RecipeItem
                    {
                        title = "Fried Egg",
                        image_url = "https://static.food2fork.com/2040646_MEDIUM7fd6.jpg",
                        tag = "egg",
                        publisher = "Sannan Hafeez",
                        steps = "1. Fry the egg 2. Eat the egg",
                    }


                );
                context.SaveChanges();
            }
        }
    }
}
