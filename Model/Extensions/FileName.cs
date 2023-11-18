using System;

namespace Model.Extensions;

public static class EnumExtensions
{
    public static bool HasAnyFlag(this Enum a, Enum b) => (Convert.ToInt16(a) & Convert.ToInt16(b)) > 0;
}
