using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TipRecipe.Services;
using static System.Net.Mime.MediaTypeNames;

namespace TipRecipe.Controllers
{
    [ApiController]
    [Route("api/blob")]
    [Authorize("Admin")]
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
            string sas = _azureBlobService.GenerateSasToken();
            var containers = await _azureBlobService.GetAllContainersAsync();
            return Ok(new { containers, sas });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            string uri = string.Empty;
            using (var stream = file.OpenReadStream())
            {
                uri = await _azureBlobService.UploadFileAsync("test", $"files/{DateTime.Now.DayOfYear.ToString()}/{file.FileName}", stream, new());
            }
            return Ok(_azureBlobService.GenerateSasTokenPolicy(uri));
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var stream = await _azureBlobService.DownloadFileAsync("test", fileName);
            return File(stream, "application/octet-stream", fileName);
        }
    }
}
