using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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
using TipRecipe.Filters;
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
        [TypeFilter(typeof(DtoResultFilterAttribute<IList<Dish>, IList<DishDto>>))]
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
            return Ok(dishList);
        }

        [HttpGet("{dishID}", Name = "GetDishByIdAsync")]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish,DishDto>))]
        public async Task<IActionResult> GetDishByIdAsync([FromRoute]string dishID)
        {
            Dish? dish = await this._dishService.GetByIdAsync(dishID);
            if(dish == null)
            {
                return NotFound();
            }
            return Ok(dish);
        }

        [HttpPost]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish, DishDto>))]
        public async Task<IActionResult> CreateDishAsync([FromBody] CreateDishDto createDishDto)
        {
            Dish dish = _mapper.Map<Dish>(createDishDto);
            if (await this._dishService.AddDishAsync(dish))
            {
                dish = await this._dishService.GetByIdAsync(dish.DishID);
                return CreatedAtAction("GetDishByIdAsync", new { dishID = dish.DishID }, dish);
            }
            return Problem();
        }

        [HttpPut("{dishID}")]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish, DishDto>))]
        public async Task<IActionResult> UpdateDishAsync(
            [FromRoute] string dishID,[FromBody] CreateDishDto updateDishDto)
        {
            Dish dish = _mapper.Map<Dish>(updateDishDto);
            if (await this._dishService.UpdateDishAsync(dishID, dish))
            {
                dish = await this._dishService.GetByIdAsync(dishID);
                return CreatedAtAction("GetDishByIdAsync", new { dishID = dish.DishID }, dish);
            }
            return NotFound();
        }

        [HttpPatch("{dishID}")]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish, DishDto>))]
        public async Task<IActionResult> PatchDishAsync(
            [FromRoute] string dishID, [FromBody] JsonPatchDocument<Dish> patchDoc)
        {
            if (patchDoc != null)
            {
                //var customer = CreateCustomer();

                //patchDoc.ApplyTo(customer, ModelState);

                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                //return new ObjectResult(customer);
                return Ok(patchDoc.ToString());
            }
            else
            {
                return BadRequest(ModelState);
            }
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
