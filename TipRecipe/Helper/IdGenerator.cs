using Microsoft.IdentityModel.Tokens;

namespace TipRecipe.Helper
{
    public static class IdGenerator
    {
        public static string GenerateDishID()
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

        public static string GenerateUserID()
        {
            int fixedLength = 40;
            string prefix = "USER";
            string preId = prefix + System.Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            if (preId.Length > fixedLength)
            {
                preId = preId.Substring(0, fixedLength);
            }
            else if (preId.Length < fixedLength)
            {
                preId = preId.PadRight(fixedLength, 'X');
            }
            preId = Base64UrlEncoder.Encode(preId);
            return preId;
        }
    }
}
