namespace online_courses.Response
{
    public interface IBaseResponse<T>
    {
        StatusCode StatusCode { get; }
        T Data { get; }
        string Description { get; }
    }
}