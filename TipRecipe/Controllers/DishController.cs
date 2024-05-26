using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TipRecipe.Entities;
using TipRecipe.Filters;
using TipRecipe.Interfaces;
using TipRecipe.Models;
using TipRecipe.Models.Dto;
using TipRecipe.Models.HttpExceptions;
using TipRecipe.Services;

namespace TipRecipe.Controllers
{
    [Route("api/dish")]
    [ApiController]
    [Authorize("User")]
    public class DishController : MyControllerBase
    {

        private readonly IMapper _mapper;

        private readonly DishService _dishService;
        private readonly CachingFileService _cachingFileService;

        public DishController(
            IMapper mapper,
            DishService dishService,
            CachingFileService cachingFileService)
        {
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
        [TypeFilter(typeof(DtoResultFilterAttribute<IEnumerable<Dish>, IEnumerable<DishDto>>))]
        public async Task<IActionResult> GetDishWithFilterAsync(
            string query = "",
            string ingredients = "",
            string types = "",
            int offset = 0,
            int limit = 5,
            string orderBy = ""
            )
        {
            var userID = User.Claims.Where(claim => claim.Type == ClaimTypes.NameIdentifier).First().Value;
            IEnumerable<Dish> dishList = await _dishService.GetDishWithFilterAsync(query,ingredients,types,offset,limit, orderBy, userID);
            return Ok(dishList);
        }

        [HttpGet("{dishID}", Name = "GetDishByIdAsync")]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish,DishDto>))]
        public async Task<IActionResult> GetDishByIdAsync([FromRoute]string dishID)
        {
            var userID = User.Claims.Where(claim => claim.Type == ClaimTypes.NameIdentifier).First().Value;
            Dish? dish = await this._dishService.GetDishWithRatingByIDAsync(dishID, userID);
            if(dish is null)
            {
                return NotFound();
            }
            return Ok(dish);
        }

        [HttpPost("rating")]
        public async Task<IActionResult> RatingDishAsync([FromBody]DishRatingDto dishRatingDto)
        {
            try
            {
                var userID = User.Claims.Where(claim => claim.Type == ClaimTypes.NameIdentifier).First().Value;
                if (await _dishService.RatingDishAsync(dishRatingDto.DishID, dishRatingDto.RatingScore, userID))
                {
                    return NoContent();
                }
                return Problem();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }


    }
}
