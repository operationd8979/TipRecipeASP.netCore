using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TipRecipe.Services;
using static System.Net.Mime.MediaTypeNames;

namespace TipRecipe.Controllers
{
    [Route("api/blob")]
    [ApiController]
    public class BlobController : ControllerBase
    {
        private readonly AzureBlobService _azureBlobService;

        public BlobController(AzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService;
        }

        [HttpGet("containers")]
        public async Task<IActionResult> GetContainers()
        {
            var containers = await _azureBlobService.GetAllContainersAsync();
            return Ok(containers);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            if (file.Length > 1024 * 1024 )
            {
                return BadRequest("File bigger than 1mbs.");
            }
            var validImageTypes = new List<string> { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/svg+xml", "image/webp" };
            if (!validImageTypes.Contains(file.ContentType))
            {
                return BadRequest("Invalid file type. Only JPEG, PNG, GIF, BMP, SVG, and WEBP are allowed.");
            }

            string uri = string.Empty;
            using (var stream = file.OpenReadStream())
            {
                uri = await _azureBlobService.UploadFileAsync("test", file.FileName, stream, new());
            }
            return Ok(uri);
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var stream = await _azureBlobService.DownloadFileAsync("test", fileName);
            return File(stream, "application/octet-stream", fileName);
        }
    }
}
