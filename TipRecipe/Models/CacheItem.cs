namespace TipRecipe.Models
{
    public class CacheItem
    {
        public object? Value { get; set; }
        public DateTime Expiration { get; set; }

        public CacheItem()
        {

        }

        public CacheItem(object value, DateTime dateTime)
        {
            Value = value;
            Expiration = dateTime;
        }
    }
}
