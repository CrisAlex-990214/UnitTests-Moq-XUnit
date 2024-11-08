namespace CleanArchitecture.Application
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T Value { get; set; }
        public string[] Messages { get; set; }
    }
}
