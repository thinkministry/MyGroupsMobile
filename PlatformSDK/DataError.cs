using System;


namespace PlatformSDK
{
    public class DataError
    {
        public enum ValidationErrorType
        {
            RequiredField,
            InvalidFormat,
            InvalidSize
        }
        public DataField Field { get; set; }

        public ValidationErrorType ErrorType { get; set; }
    }
}
