using System;
using System.ComponentModel;

namespace TritonSim.GUI.Infrastructure
{
    public static class ExtensionMethods
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();

            var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(
                field,
                typeof(DescriptionAttribute)
            );

            return attr?.Description ?? value.ToString();
        }
    }
}
