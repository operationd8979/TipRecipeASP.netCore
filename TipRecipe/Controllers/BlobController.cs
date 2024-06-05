using Microsoft.AspNetCore.Mvc;
using TipRecipe.Services;

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

            string uri = string.Empty;

            using (var stream = file.OpenReadStream())
            {
                uri = await _azureBlobService.UploadFileAsync("test", file.FileName, stream);
            }

            return Ok(uri ?? "File uploaded successfully.");
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var stream = await _azureBlobService.DownloadFileAsync("test", fileName);
            return File(stream, "application/octet-stream", fileName);
        }
    }
}
