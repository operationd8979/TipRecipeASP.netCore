using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using TipRecipe.Entities;
using TipRecipe.Helper;
using TipRecipe.Interfaces;
using TipRecipe.Models;
using TipRecipe.Models.Dto;
using TipRecipe.Services;

namespace TipRecipe.Controllers
{
    [Route("api/dish")]
    [ApiController]
    public class DishController : MyControllerBase
    {

        private readonly ILogger<DishController> _logger;
        private readonly ITranslateMapper<Dish, DishDto> _dishTranslateMapper;
        private readonly IMapper _mapper;

        private readonly DishService _dishService;


        public DishController(
            ILogger<DishController> logger,
            ITranslateMapper<Dish, DishDto> dishTranslateMapper,
            IMapper mapper,
            DishService dishService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dishTranslateMapper = dishTranslateMapper ?? throw new ArgumentNullException(nameof(dishTranslateMapper));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
        }

        [HttpGet]
        public async Task<IActionResult> GetDishWithFilterAsync(
            string query = "",
            string ingredients = "",
            string types = "",
            int offset = 0,
            int limit = 5,
            string orderBy = ""
            )
        {
            IList<Dish> dishList = (await _dishService.GetDishWithFilterAsync(query,ingredients,types,offset,limit, orderBy)).ToList();
            return Ok(this._mapper.Map<IList<DishDto>>(dishList));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDishByIdAsync([FromRoute]string id)
        {
            Dish dish = await this._dishService.GetByIdAsync(id);
            return Ok(this._mapper.Map<DishDto>(dish));
        }

        [HttpPost]
        public async Task<IActionResult> CreateDishAsync([FromBody] DishDto? dishDto)
        {
            Dish dish = _mapper.Map<Dish>(dishDto);
            if (await this._dishService.AddDishAsync(dish) >= 0)
            {
                dish = await this._dishService.GetByIdAsync(dish.DishID);
                return CreatedAtAction("GetDishByIdAsync", new { id = dish.DishID }, _mapper.Map<DishDto>(dish));
            }
            return Problem();
        }


        [HttpPost("file")]
        public async Task GetFileAsync(IFormFile file)
        {
            if (file.Length == 0 || file.Length > 1024 * 1024)
            {
                throw new Exception("File size is invalid");
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Guid.NewGuid().ToString()+".pdf");
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        
    }
}
