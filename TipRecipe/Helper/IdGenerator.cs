using Microsoft.IdentityModel.Tokens;

namespace TipRecipe.Helper
{
    public static class IdGenerator
    {
        public static string GenerateDishId()
        {
            int fixedLength = 40;
            string prefix = "DISH";
            string preId = prefix + System.Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            if (preId.Length > fixedLength)
            {
                preId = preId.Substring(0, fixedLength);
            }
            else if(preId.Length < fixedLength)
            {
                preId = preId.PadRight(fixedLength, 'X');
            }
            preId = Base64UrlEncoder.Encode(preId);
            return preId;
        }
    }
}
