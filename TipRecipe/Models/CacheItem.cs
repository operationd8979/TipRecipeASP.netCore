namespace TipRecipe.Models
{
    public class CacheItem
    {
        public object Value { get; set; }
        public DateTime Expiration { get; set; }
    }
}
