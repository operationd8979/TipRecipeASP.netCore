using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TipRecipe.Entities;
using TipRecipe.Filters;
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

        public DishController(
            IMapper mapper,
            DishService dishService,
            CachingFileService cachingFileService,
            AzureBlobService azureBlobService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
        }

        [HttpGet("async")]
        [TypeFilter(typeof(DtoResultFilterAttribute<IEnumerable<Dish>, IEnumerable<DishDto>>))]
        [TypeFilter(typeof(AddSasBlobFilterAttribute))]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _dishService.GetAllAsync());
        }

        [HttpGet("asyncEnumerable")]
        [TypeFilter(typeof(AddSasBlobFilterAttribute))]
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
        [TypeFilter(typeof(AddSasBlobFilterAttribute))]
        public async Task<IActionResult> GetDishWithFilterAsync(
            string query = "",
            string ingredients = "",
            string types = "",
            int offset = 0,
            int limit = 5,
            string orderBy = ""
            )
        {
            var userID = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            IEnumerable<Dish> dishList = await _dishService.GetDishWithFilterAsync(query,ingredients,types,offset,limit, orderBy, userID);
            return Ok(dishList);
        }

        [HttpGet("recommend")]
        [TypeFilter(typeof(DtoResultFilterAttribute<IEnumerable<Dish>, IEnumerable<DishDto>>))]
        [TypeFilter(typeof(AddSasBlobFilterAttribute))]
        public async Task<IActionResult> GetRecommendDishes()
        {
            var userID = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            return Ok(await _dishService.GetRecommendDishesAsync(userID));
        }

        [HttpGet("{dishID}", Name = "GetDishByIdAsync")]
        [TypeFilter(typeof(DtoResultFilterAttribute<Dish,DishDto>))]
        [TypeFilter(typeof(AddSasBlobFilterAttribute))]
        public async Task<IActionResult> GetDishByIdAsync([FromRoute]string dishID)
        {
            var userID = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
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
                var userID = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
                if (await _dishService.RatingDishAsync(dishRatingDto.DishID, dishRatingDto.RatingScore??0f, userID))
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
