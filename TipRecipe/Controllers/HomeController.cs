﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TipRecipe.Entities;
using TipRecipe.Filters;
using TipRecipe.Models;
using TipRecipe.Models.Dto;
using TipRecipe.Services;

namespace TipRecipe.Controllers
{
    [Route("api")]
    [ApiController]
    public class HomeController : MyControllerBase
    {

        private readonly DishService _dishService;
        private readonly IMapper _mapper;

        public HomeController(IMapper mapper, DishService dishService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
        }

        [HttpGet("ingredients")]
        [TypeFilter(typeof(DtoResultFilterAttribute<IList<Ingredient>, IList<IngredientDto>>))]
        public async Task<IActionResult> GetIngredientsAsync()
        {
            IList<Ingredient> ingredients = (await this._dishService.GetIngredientsAsync()).ToList();
            return Ok(ingredients);
        }

        [HttpGet("types")]
        [TypeFilter(typeof(DtoResultFilterAttribute<IList<TypeDish>, IList<TypeDishDto>>))]
        public async Task<IActionResult> GetTypesAsync()
        {
            IList<TypeDish> types = (await this._dishService.GetTypesAsync()).ToList();
            return Ok(types);
        }


    }
}