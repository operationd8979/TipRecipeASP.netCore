using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TipRecipe.Entities;
using TipRecipe.Filters;
using TipRecipe.Models;
using TipRecipe.Models.Dto;
using TipRecipe.Services;

namespace TipRecipe.Controllers
{
    [ApiController]
    [Authorize("Admin")]
    [Route("api/admin")]
    public class AdminController : MyControllerBase
    {

        private readonly IMapper _mapper;

        private readonly DishService _dishService;

        public AdminController(IMapper mapper, DishService dishService)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
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
            int total = await this._dishService.GetCountDishesAsync();
            return Ok(new AdminDishDto(dishDtos,total));
        }

        [HttpPost("dish")]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish, DishDto>))]
        public async Task<IActionResult> CreateDishAsync([FromBody] CreateDishDto createDishDto)
        {
            Dish? dish = _mapper.Map<Dish>(createDishDto);
            if (await this._dishService.AddDishAsync(dish))
            {
                dish = await this._dishService.GetByIdAsync(dish.DishID) ?? throw new ArgumentNullException(nameof(dish));
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
        public async Task<IActionResult> UpdateDishAsync(
            [FromRoute] string dishID, [FromBody] CreateDishDto updateDishDto)
        {
            Dish? dish = _mapper.Map<Dish>(updateDishDto);
            if (await this._dishService.UpdateDishAsync(dishID, dish))
            {
                dish = await this._dishService.GetByIdAsync(dishID) ?? throw new ArgumentNullException(nameof(dish));
                return CreatedAtAction(
                    nameof(DishController.GetDishByIdAsync), 
                    controllerName: "Dish",
                    new { dishID = dish.DishID }, 
                    dish);
            }
            return NotFound();
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

        [HttpPost("file")]
        public async Task GetFileAsync(IFormFile file)
        {
            if (file.Length == 0 || file.Length > 1024 * 1024)
            {
                throw new Exception("File size is invalid");
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Guid.NewGuid().ToString() + ".pdf");
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

    }
}
