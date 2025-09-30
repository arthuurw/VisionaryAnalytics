using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace VisionaryAnalytics.Application.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayName = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName();

            return displayName ?? enumValue.ToString();
        }
    }
}
