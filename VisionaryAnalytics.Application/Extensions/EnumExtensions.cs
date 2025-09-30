using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace VisionaryAnalytics.Application.Extensions
{
    public static class EnumExtensions
    {
        public static string ObterNomeExibicao(this Enum valorEnum)
        {
            var displayName = valorEnum.GetType()
                .GetMember(valorEnum.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName();

            return displayName ?? valorEnum.ToString();
        }
    }
}
