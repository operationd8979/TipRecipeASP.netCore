using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

namespace TipRecipe.Controllers
{
    [Route("api/dish")]
    [ApiController]
    public class DishController : MyControllerBase
    {

        private readonly ILogger<DishController> _logger;
        private readonly ITranslateMapper<Dish, DishDto> _dishTranslateMapper;
        private readonly IMapper _mapper;

        private IList<Dish> DishList = new List<Dish>
        {
            new Dish("1","Dish 1","Description 1","URL 1"),
            new Dish("1","Dish 1","Description 1","URL 1"),
            new Dish("2","Dish 2","Description 2","URL 2"),
            new Dish("3","Dish 3","Description 3","URL 3"),
            new Dish("4","Dish 4","Description 4","URL 4"),
            new Dish("5","Dish 5","Description 5","URL 5"),
            new Dish("6","Dish 6","Description 6","URL 6"),
            new Dish("7","Dish 7","Description 7","URL 7"),
            new Dish("8","Dish 8","Description 8","URL 8"),
            new Dish("9","Dish 9","Description 9","URL 9"),
            new Dish("10","Dish 10","Description 10","URL 10")
        };

        public DishController(
            ILogger<DishController> logger,
            ITranslateMapper<Dish, DishDto> dishTranslateMapper,
            IMapper mapper)
        {
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _dishTranslateMapper = dishTranslateMapper?? throw new ArgumentNullException(nameof(dishTranslateMapper));
            _mapper = mapper?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDishs([FromQuery] IDictionary<string, string> query)
        {
            string[] SoftFields = new string[] { "search", "ingredients", "types" };
            string[] HardFeilds = new string[] { "offset", "limit" };
            IDictionary<string, string> SoftFieldsDic = new Dictionary<string, string>();
            IDictionary<string, string> HardFieldsDic = new Dictionary<string, string>();
            foreach (var q in query)
            {
                if(SoftFields.Contains(q.Key))
                {
                    SoftFieldsDic.Add(q.Key, q.Value);
                    _logger.LogInformation($"add soft [{q.Key}] : {q.Value}");
                }
                else if(HardFeilds.Contains(q.Key))
                {
                    HardFieldsDic.Add(q.Key, q.Value);
                    _logger.LogInformation($"add hard [{q.Key}] : {q.Value}");
                }
            }
            return Ok(_mapper.Map<IList<DishDto>>(DishList));
            //return Ok(_dishTranslateMapper.TranslateList(DishList));
        }


        [HttpGet("test")]
        public async Task<IActionResult> test([FromQuery] IDictionary<string, string> query)
        {
            List<string> strings = new List<string>();
            foreach (var q in query)
            {
                var parameter = q.Key;
                var value = q.Value;

                strings.Add($"{parameter} : {value}");
            }

            return Ok(strings);
        }

        [HttpGet("void")]
        public async Task PrintTime()
        {
            Console.WriteLine("Time: " + DateTime.Now);
        }

        [HttpPost("file")]
        public async Task GetFile(IFormFile file)
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
