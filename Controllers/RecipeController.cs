using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using RecipeBank.Helpers;
using RecipeBank.Models;

namespace RecipeBank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly RecipeBankContext _context;

        private IConfiguration _configuration;

        public RecipeController(RecipeBankContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Recipe
        [HttpGet]
        public IEnumerable<RecipeItem> GetRecipeItem()
        {
            return _context.RecipeItem;
        }


        // PUT: api/Recipe/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipeItem([FromRoute] int id, [FromBody] RecipeItem recipeItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != recipeItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(recipeItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecipeItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Recipe
        [HttpPost]
        public async Task<IActionResult> PostRecipeItem([FromBody] RecipeItem recipeItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.RecipeItem.Add(recipeItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecipeItem", new { id = recipeItem.Id }, recipeItem);
        }

        // DELETE: api/Recipe/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipeItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var recipeItem = await _context.RecipeItem.FindAsync(id);
            if (recipeItem == null)
            {
                return NotFound();
            }

            _context.RecipeItem.Remove(recipeItem);
            await _context.SaveChangesAsync();

            return Ok(recipeItem);
        }

        // GET: api/Recipe/tag
        [HttpGet("{tag}")]
        public IActionResult GetRecipeItem([FromRoute] string tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var recipeItem = _context.RecipeItem.Where(s => s.tag == tag);

            if (recipeItem == null)
            {
                return NotFound();
            }

            return Ok(recipeItem);
        }

        [HttpPost, Route("upload")]
        public async Task<IActionResult> UploadFile([FromForm]RecipeImageItem recipe)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            try
            {
                using (var stream = recipe.image_url.OpenReadStream())
                {
                    var cloudBlock = await UploadToBlob(recipe.image_url.FileName, null, stream);
                    //// Retrieve the filename of the file you have uploaded
                    //var filename = provider.FileData.FirstOrDefault()?.LocalFileName;
                    if (string.IsNullOrEmpty(cloudBlock.StorageUri.ToString()))
                    {
                        return BadRequest("An error has occured while uploading your file. Please try again.");
                    }

                    RecipeItem recipeItem = new RecipeItem();
                    recipeItem.publisher = recipe.publisher;
                    recipeItem.steps = recipe.steps;
                    recipeItem.title = recipe.title;
                    recipeItem.tag = recipe.tag;

                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                    recipeItem.image_url = cloudBlock.SnapshotQualifiedUri.AbsoluteUri;

                    _context.RecipeItem.Add(recipeItem);
                    await _context.SaveChangesAsync();

                    return Ok($"File: {recipe.title} has successfully uploaded");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error has occured. Details: {ex.Message}");
            }


        }

        private async Task<CloudBlockBlob> UploadToBlob(string filename, byte[] imageBuffer = null, System.IO.Stream stream = null)
        {

            var accountName = _configuration["AzureBlob:name"];
            var accountKey = _configuration["AzureBlob:key"]; ;
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer imagesContainer = blobClient.GetContainerReference("images");

            string storageConnectionString = _configuration["AzureBlob:connectionString"];

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Generate a new filename for every new blob
                    var fileName = Guid.NewGuid().ToString();
                    fileName += GetFileExtention(filename);

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = imagesContainer.GetBlockBlobReference(fileName);

                    if (stream != null)
                    {
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                    }
                    else
                    {
                        return new CloudBlockBlob(new Uri(""));
                    }

                    return cloudBlockBlob;
                }
                catch (StorageException ex)
                {
                    return new CloudBlockBlob(new Uri(""));
                }
            }
            else
            {
                return new CloudBlockBlob(new Uri(""));
            }

        }

        private string GetFileExtention(string fileName)
        {
            if (!fileName.Contains("."))
                return ""; //no extension
            else
            {
                var extentionList = fileName.Split('.');
                return "." + extentionList.Last(); //assumes last item is the extension 
            }
        }


        private bool RecipeItemExists(int id)
        {
            return _context.RecipeItem.Any(e => e.Id == id);
        }
    }
}