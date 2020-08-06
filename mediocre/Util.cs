namespace Mediocre {
    using System.Collections.Generic;

    public static class Util {
        public static string Join(this IEnumerable<string> strs, string sep = ", ") => string.Join(sep, strs);
    }
}
