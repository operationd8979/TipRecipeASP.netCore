using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TipRecipe.Entities;
using TipRecipe.Filters;
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
        private readonly CachingFileService _cachingFileService;

        public DishController(
            ILogger<DishController> logger,
            ITranslateMapper<Dish, DishDto> dishTranslateMapper,
            IMapper mapper,
            DishService dishService,
            CachingFileService cachingFileService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dishTranslateMapper = dishTranslateMapper ?? throw new ArgumentNullException(nameof(dishTranslateMapper));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _cachingFileService = cachingFileService ?? throw new ArgumentNullException(nameof(cachingFileService));
        }

        [HttpGet("async")]
        [TypeFilter(typeof(DtoResultFilterAttribute<IEnumerable<Dish>, IEnumerable<DishDto>>))]
        public async Task<IActionResult> GetAllAsync()
        {
            var cachedDishes = await _cachingFileService.GetAsync<IEnumerable<DishDto>>("alldishes");
            if (cachedDishes != null)
            {
                return Ok(cachedDishes);
            }
            cachedDishes = _mapper.Map<IEnumerable<DishDto>>(await _dishService.GetAllAsync());
            await _cachingFileService.SetAsync("alldishes", cachedDishes, TimeSpan.FromMinutes(15));
            return Ok(cachedDishes);
        }

        [HttpGet("asyncEnumerable")]
        public async IAsyncEnumerable<DishDto> GetAllEnumerableAsync()
        {
            IAsyncEnumerable<Dish> dishList = _dishService.GetAllEnumerableAsync();
            await foreach (var dish in dishList)
            {
                Task.Delay(500).Wait();
                yield return _mapper.Map<DishDto>(dish);
            }
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
            Dish? dish = _mapper.Map<Dish>(createDishDto);
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
            Dish? dish = _mapper.Map<Dish>(updateDishDto);
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
                Dish? dish = await this._dishService.GetByIdAsync(dishID);
                if (dish == null)
                {
                    return NotFound();
                }
                patchDoc.ApplyTo(dish, ModelState);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                _dishService.SaveChanges();
                return CreatedAtAction("GetDishByIdAsync", new { dishID = dish.DishID }, dish);
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

        [HttpGet("ratings")]
        public async Task<IActionResult> GetUserDishRatings()
        {
            var cachedRatings = await _cachingFileService.GetAsync<IEnumerable<UserDishRating>>("ratings");
            if (cachedRatings != null)
            {
                return Ok(cachedRatings);
            }
            cachedRatings = await _dishService.GetUserDishRatingsAsync();
            await _cachingFileService.SetAsync("ratings", cachedRatings, TimeSpan.FromMinutes(15));
            return Ok(cachedRatings);
        }


    }
}
