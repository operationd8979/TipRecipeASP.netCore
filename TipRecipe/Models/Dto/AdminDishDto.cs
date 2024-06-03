namespace TipRecipe.Models.Dto
{
    public record AdminDishDto(IEnumerable<DishDto> Dishes, int Total);
}
