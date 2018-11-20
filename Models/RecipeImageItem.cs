using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeBank.Models
{
    public class RecipeImageItem
    {
        public string publisher { get; set; }
        public string title { get; set; }
        public IFormFile image_url { get; set; }
        public string steps { get; set; }
        public string tag { get; set; }
    }
}
