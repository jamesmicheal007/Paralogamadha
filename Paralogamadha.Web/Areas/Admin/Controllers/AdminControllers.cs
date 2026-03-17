// Needed extension method placeholder - in actual project this would use proper DI resolution
namespace System.Linq
{
    public static class EnumerableExtensionForAdmin
    {
        public static Paralogamadha.Core.Models.ApplicationUser FirstOrDefault(
            this System.Collections.Generic.IEnumerable<Paralogamadha.Core.Models.ApplicationUser> source,
            System.Func<Paralogamadha.Core.Models.ApplicationUser, bool> predicate)
        {
            foreach (var item in source) if (predicate(item)) return item;
            return null;
        }
    }
}
