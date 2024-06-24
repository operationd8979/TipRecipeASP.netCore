using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.ComponentModel.DataAnnotations;
using TipRecipe.Entities;
using TipRecipe.Filters;
using TipRecipe.Models;
using TipRecipe.Models.Dto;
using TipRecipe.Services;

namespace TipRecipe.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize("Admin")]
    public class AdminController : MyControllerBase
    {
        private readonly List<string> VALIDIMAGETYPES = new List<string> { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/svg+xml", "image/webp" };
        private readonly string MESSAGE_INVALID_PARAMETER = "Invalid parameters";
        private readonly string MESSAGE_FILE_NOT_FOUND = "File not found";
        private readonly string MESSAGE_INVALID_FILE = "Invalid file type. Only JPEG, PNG, GIF, BMP, SVG, and WEBP are allowed";
        private readonly string MESSAGE_FILE_BIGGER = "File bigger than 1mbs";
        private readonly string MESSAGE_FILE_UPLOAD_FAILED = "Upload file failed";

        private readonly IMapper _mapper;

        private readonly DishService _dishService;
        private readonly AzureBlobService _azureBlobService;

        public AdminController(IMapper mapper, DishService dishService, AzureBlobService azureBlobService)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            this._azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        [HttpGet("dish")]
        public async Task<IActionResult> GetAllDishesAsync(
            string query = "",
            int offset = 0,
            int limit = 5,
            string orderBy = ""
            )
        {
            IEnumerable<Dish> dishes = await this._dishService.GetDishByAdminAsync(query,offset,limit,orderBy);
            IEnumerable<DishDto> dishDtos = _mapper.Map<IEnumerable<DishDto>>(dishes);
            for(int i = 0; i < dishDtos.Count(); i++)
            {
                dishDtos.ElementAt(i).UrlPhoto = _azureBlobService.GenerateSasTokenPolicy(dishDtos.ElementAt(i).UrlPhoto!);
            }
            int total = await this._dishService.GetCountDishesAsync(query);
            return Ok(new AdminDishDto(dishDtos,total));
        }

        [HttpPost("dish")]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish, DishDto>))]
        [TypeFilter(typeof(AddSasBlobFilterAttribute))]
        public async Task<IActionResult> CreateDishAsync([FromForm] string dishName, [FromForm] string summary,
                                                 [FromForm] string detailIngredientDishes, [FromForm] string detailTypeDishes,
                                                 [FromForm] string recipe, [FromForm] IFormFile file)
        {
            ICollection<DetailIngredientDishDto> detailIngredientDishesList = JsonConvert.DeserializeObject<ICollection<DetailIngredientDishDto>>(detailIngredientDishes)!;
            ICollection<DetailTypeDishDto> detailTypeDishesList = JsonConvert.DeserializeObject<ICollection<DetailTypeDishDto>>(detailTypeDishes)!;
            RecipeDto recipeObj = JsonConvert.DeserializeObject<RecipeDto>(recipe)!;
            if(detailIngredientDishesList.Count == 0 || detailTypeDishesList.Count == 0 || recipeObj == null)
            {
                return BadRequest(MESSAGE_INVALID_PARAMETER);
            }
            var createDishDto = new CreateDishDto(dishName, summary, "", detailIngredientDishesList, detailTypeDishesList, recipeObj);
            Dish? dish = _mapper.Map<Dish>(createDishDto);
            if (IsInvalidFileImageDish(file, out string message))
            {
                return BadRequest(message);
            }
            string uri = string.Empty;
            using (var stream = file.OpenReadStream())
            {
                Dictionary<string,string> tags = new Dictionary<string, string>
                {
                    { "Project", "TestRecipe"},
                    { "Environment", "Developer"},
                    { "Department", "Hyperscale" },
                    { "Item" , "DishPhoto" },
                    { "DishID", dish.DishID }
                };
                uri = await _azureBlobService.UploadFileAsync("test", dish.DishID, stream, tags);
            }
            if (string.IsNullOrEmpty(uri))
            {
                return BadRequest(MESSAGE_FILE_UPLOAD_FAILED);
            }
            dish.UrlPhoto = uri;
            if (await this._dishService.AddDishAsync(dish))
            {
                dish = await this._dishService.GetByIdAsync(dish.DishID) ?? 
                    throw new KeyNotFoundException($"Dish with ID {dish.DishID} not found");
                return CreatedAtAction(
                    nameof(DishController.GetDishByIdAsync),
                    controllerName: "Dish",
                    new { dishID = dish.DishID },
                    dish);
            }
            return Problem();
        }

        [HttpPut("dish/{dishID}")]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish, DishDto>))]
        [TypeFilter(typeof(AddSasBlobFilterAttribute))]
        public async Task<IActionResult> UpdateDishAsync([FromRoute] string dishID, 
                                                 [FromForm] string dishName, [FromForm] string summary,
                                                 [FromForm] string detailIngredientDishes, [FromForm] string detailTypeDishes,
                                                 [FromForm] string recipe, [FromForm] IFormFile? file)
        {
            ICollection<DetailIngredientDishDto> detailIngredientDishesList = JsonConvert.DeserializeObject<ICollection<DetailIngredientDishDto>>(detailIngredientDishes)!;
            ICollection<DetailTypeDishDto> detailTypeDishesList = JsonConvert.DeserializeObject<ICollection<DetailTypeDishDto>>(detailTypeDishes)!;
            RecipeDto recipeObj = JsonConvert.DeserializeObject<RecipeDto>(recipe)!;
            if (detailIngredientDishesList.Count == 0 || detailTypeDishesList.Count == 0 || recipeObj == null)
            {
                return BadRequest(MESSAGE_INVALID_PARAMETER);
            }
            var createDishDto = new CreateDishDto(dishName, summary, "", detailIngredientDishesList, detailTypeDishesList, recipeObj);
            Dish? dish = _mapper.Map<Dish>(createDishDto);
            if(file is not null)
            {
                if(IsInvalidFileImageDish(file,out string message))
                {
                    return BadRequest(message);
                }
                string uri = string.Empty;
                using (var stream = file.OpenReadStream())
                {
                    Dictionary<string, string> tags = new Dictionary<string, string>
                {
                    { "Project", "TestRecipe"},
                    { "Environment", "Developer"},
                    { "Department", "Hyperscale" },
                    { "Item" , "DishPhoto" },
                    { "DishID", dish.DishID }
                };
                    uri = await _azureBlobService.UploadFileAsync("test", dishID, stream, tags);
                }
                if (string.IsNullOrEmpty(uri))
                {
                    return BadRequest(MESSAGE_FILE_UPLOAD_FAILED);
                }
                dish.UrlPhoto = uri;
            }
            if (await this._dishService.UpdateDishAsync(dishID, dish))
            {
                dish = await this._dishService.GetByIdAsync(dishID) ?? 
                    throw new KeyNotFoundException($"Dish with ID {dishID} not found");
                return CreatedAtAction(
                    nameof(DishController.GetDishByIdAsync),
                    controllerName: "Dish",
                    new { dishID = dish.DishID },
                    dish);
            }
            return Problem();
        }

        [HttpPatch("dish/{dishID}")]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish, DishDto>))]
        public async Task<IActionResult> PatchDishAsync(
            [FromRoute] string dishID, [FromBody] JsonPatchDocument<Dish> patchDoc)
        {
            if (patchDoc != null)
            {
                Dish? dish = await this._dishService.GetByIdAsync(dishID);
                if (dish is null)
                {
                    return NotFound();
                }
                patchDoc.ApplyTo(dish, ModelState);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                _dishService.SaveChanges();
                return CreatedAtAction(
                    nameof(DishController.GetDishByIdAsync),
                    controllerName: "Dish",
                    new { dishID = dish.DishID },
                    dish);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpDelete("{dishID}")]
        public async Task<IActionResult> DeleteDishAsync([FromRoute] string dishID)
        {
            if (await this._dishService.DeleteDishAsync(dishID))
            {
                return NoContent();
            }
            return NotFound();
        }

        private bool IsInvalidFileImageDish(IFormFile file,out string message)
        {
            if (file == null || file.Length == 0)
            {
                message = MESSAGE_FILE_NOT_FOUND;
                return true;
            }
            if (file.Length > 1024 * 1024)
            {
                message = MESSAGE_FILE_BIGGER;
                return true;
            }
            if (!VALIDIMAGETYPES.Contains(file.ContentType))
            {
                message = MESSAGE_INVALID_FILE;
                return true;
            }
            message = string.Empty;
            return false;
        }


    }
}
