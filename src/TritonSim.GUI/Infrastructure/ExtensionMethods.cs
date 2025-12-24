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

        public static string ToFriendlyError(this ResponseCode code)
        {
            var message = code.GetDescription();
            if (code.IsFailure())
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
            return state.IsReadyToInit() && (state.HasFlag(RendererInitState.NativeInitSuccess) || state.HasFlag(RendererInitState.NativeInitFailed));
        }
    }
}
