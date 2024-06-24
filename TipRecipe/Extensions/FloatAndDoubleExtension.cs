namespace TipRecipe.Extensions
{
    public static class FloatAndDoubleExtension
    {
        public static bool IsApproximately(this float self, double other)
        {
            float epsilon = 0.0001f;
            return Math.Abs(self - other) <= epsilon;
        }

        public static bool IsApproximately(this double self, double other)
        {
            float epsilon = 0.0001f;
            return Math.Abs(self - other) <= epsilon;
        }

        public static bool IsApproximately(this double self, double other, double within)
        {
            double epsilon = 0.0001d;
            return Math.Abs(self - other) <= epsilon;
        }

        public static bool IsApproximately(this float self, float other, float within)
        {
            return Math.Abs(self - other) <= within;
        }
    }
}
