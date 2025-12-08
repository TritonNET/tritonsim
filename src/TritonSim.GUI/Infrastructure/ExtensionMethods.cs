using System;
using System.ComponentModel;
using System.Resources;
using System.Runtime.InteropServices;
using TritonSim.GUI.Resources;

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

        private static readonly ResourceManager m_resourceManager = Strings.ResourceManager;

        public static string ToLocalized(this ResponseCode code)
        {
            var resourceKey = code.ToString();
            var result = m_resourceManager.GetString(resourceKey);

            return result ?? resourceKey;
        }

        public static string ToFriendlyError(this ResponseCode code)
        {
            var isFailure = ((int)code & (int)ResponseCode.Failed) != 0;

            var message = code.ToLocalized();

            if (isFailure)
                return $"Error: {message}";

            return message;
        }

        public static bool IsFailure(this ResponseCode code)
        {
            return (int)code < 0;
        }

        public static bool IsSuccess(this ResponseCode code)
        {
            return (int)code >= 0;
        }

        public static bool IsReadyToInit(this RendererInitState state)
        {
            return
                state.HasFlag(RendererInitState.TypeSet) &&
                state.HasFlag(RendererInitState.HandleSet) &&
                state.HasFlag(RendererInitState.SizeSet) &&
                state.HasFlag(RendererInitState.AttachedToVisualTree);
        }

        public static bool IsInitCompleted(this RendererInitState state)
        {
            return state.IsReadyToInit() && state.HasFlag(RendererInitState.NativeInitialized);
        }
    }
}
