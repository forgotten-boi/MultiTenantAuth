using System.Collections.Generic;

namespace BusinessAccess
{
    public class AppScopes
    {
        public const string SCOPE1 = "scope1";
        public const string SCOPE2 = "scope2";

        public static IEnumerable<string> GetAllScopes()
        {
            yield return SCOPE1;
            yield return SCOPE2;
        }
    }
}
