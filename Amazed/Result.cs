namespace DreamAmazon
{
    public class Result
    {
        public bool Success { get; }
        public string Error { get; private set; }

        public bool Failure => !Success;

        public Result(bool success, string error)
        {
            Contracts.Require(success || !string.IsNullOrEmpty(error));
            Contracts.Require(!success || string.IsNullOrEmpty(error));

            Success = success;
            Error = error;
        }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(default(T), false, message);
        }

        public static Result Ok()
        {
            return new Result(true, string.Empty);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }

        public static Result Combine(params Result[] results)
        {
            foreach (var result in results)
            {
                if (result.Failure)
                {
                    return result;
                }
            }
            return Ok();
        }
    }

    public class Result<T> : Result
    {
        private T _value;

        public T Value
        {
            get
            {
                Contracts.Require(Success);
                return _value;
            }
            //[param: AllowNull]
            private set { _value = value; }
        }

        protected internal Result(/*[AllowNull] */T value, bool success, string error)
            : base(success, error)
        {
            Contracts.Require(value != null || !success);
            Value = value;
        }
    }
}