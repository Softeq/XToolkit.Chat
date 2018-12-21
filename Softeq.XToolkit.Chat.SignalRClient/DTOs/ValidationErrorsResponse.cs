namespace Softeq.XToolkit.Chat.SignalRClient.DTOs
{
    internal class ValidationErrorsResponse
    {
        public string PropertyName { get; set; }

        public object AttemptedValue { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public object CustomState { get; set; }
    }
}
