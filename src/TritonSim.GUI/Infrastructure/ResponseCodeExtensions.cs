using TritonSim.GUI.Resources;
using System.Resources;

namespace TritonSim.GUI.Infrastructure
{
    public static class ResponseCodeExtensions
    {
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
    }
}