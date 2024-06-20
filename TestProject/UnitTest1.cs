

using Microsoft.Extensions.Logging;
using Moq;
using TipRecipe.Interfaces;
using TipRecipe.Services;

namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        Mock<IDishRepository> mockDishRepository = new Mock<IDishRepository>();
        Mock<IIngredientRepository> mockIngredientRepository = new Mock<IIngredientRepository>();
        Mock<ITypeDishRepository> mockTypeDishRepository = new Mock<ITypeDishRepository>();
        Mock<ILogger<CachingFileService>> mockLogger = new Mock<ILogger<CachingFileService>>();

        DishService? dishService = null;

        [TestMethod]
        public void GetCountDishesAsync()
        {
            Mock<CachingFileService> mockCachingFileService = new Mock<CachingFileService>("testSample",mockLogger.Object);
            Mock<CachedRatingScoreService> mockCachedRatingService = new Mock<CachedRatingScoreService>(mockCachingFileService.Object);
            dishService = new DishService(mockDishRepository.Object, mockIngredientRepository.Object, mockTypeDishRepository.Object, mockCachedRatingService.Object);
            mockDishRepository.Setup(x => x.GetCountAsync("test")).ReturnsAsync(5);
            var result = dishService.GetCountDishesAsync("test").Result;
            Assert.AreEqual(5, result);
        }
    }
}