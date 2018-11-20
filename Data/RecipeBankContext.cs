using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RecipeBank.Models
{
    public class RecipeBankContext : DbContext
    {
        public RecipeBankContext (DbContextOptions<RecipeBankContext> options)
            : base(options)
        {
        }

        public DbSet<RecipeBank.Models.RecipeItem> RecipeItem { get; set; }
    }
}
