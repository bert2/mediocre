namespace Mediocre {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using CommandLine;

    using Mediocre.CLI;

    using YeelightAPI.Models;

    public static class Util {
        public static T With<T>(this T x, Action<T> effect) {
            effect(x);
            return x;
        }

        public static void ForEach<T>(this IEnumerable<T> xs, Action<T> effect) {
            foreach (var x in xs)
                effect(x);
        }

        public static string Join(this IEnumerable<string> strs, string sep = ", ")
            => string.Join(sep, strs);

        public static string Print(this IEnumerable<Screen> screens)
            => screens.Select(s => $"'{s.DeviceName}'").Join(", ");

        public static string Print(this Color c) => $"({c.R}, {c.G}, {c.B})";

        public static int ToRgb(this Color c) => (c.R << 16) | (c.G << 8) | c.B;

        public static int Scale(this float x, int min, int max, int? factor = null)
            => (int)Math.Round(Math.Clamp(x * (factor ?? max), min, max));

        #region hack the real name of a supported device operation

        private static readonly ConcurrentDictionary<METHODS, string> realNames = new ConcurrentDictionary<METHODS, string>();

        private static readonly Type realNameAttrType = typeof(YeelightAPI.Device).Assembly
            .GetType("YeelightAPI.Core.RealNameAttribute")
            ?? throw new ReflectionTypeLoadException(null, null, $"Unable to load internal type YeelightAPI.Core.RealNameAttribute");

        private static readonly PropertyInfo propertyNameProp = realNameAttrType.GetProperty("PropertyName")
            ?? throw new MissingMemberException("YeelightAPI.Core.RealNameAttribute", "PropertyName");

        public static string GetRealName(this METHODS method) {
            if (realNames.TryGetValue(method, out var cached)) return cached;

            var realNameAttr = typeof(METHODS)
                .GetMember(method.ToString())
                .Single()
                .GetCustomAttribute(realNameAttrType, false)
                ?? throw new MemberAccessException($"[RealName] attribute is missing on METHODS.{method}.");

            var realName = (string?)propertyNameProp.GetValue(realNameAttr)
                ?? throw new MemberAccessException($"PropertyName of [RealName] attribute for METHODS.{method} was null.");

            _ = realNames.TryAdd(method, realName);

            return realName;
        }

        #endregion

        #region hack `NotParsed<T>` into `notParsedFunc` of `MapResult()`

        public static async Task<int> MapResult(
            this ParserResult<object> result,
            Func<SyncOpts, Task<int>> parsedSync,
            Func<PrintOpts, Task<int>> parsedPrint,
            Func<ReadOpts, Task<int>> parsedRead,
            Func<ListOpts, Task<int>> parsedList,
            Func<NotParsed<object>, Task<int>> notParsed)
            => await result.MapResult(
                parsedSync,
                parsedPrint,
                parsedRead,
                parsedList,
                notParsedFunc: _ => notParsed((NotParsed<object>)result));

        #endregion
    }
}
