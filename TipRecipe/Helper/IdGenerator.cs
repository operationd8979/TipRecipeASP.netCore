namespace TipRecipe.Helper
{
    public static class IdGenerator
    {
        public static string GenerateDishId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
