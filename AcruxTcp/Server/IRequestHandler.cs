public interface IRequestHadler<TRequest, TResponse>
{
    ITcpDataCodec<TRequest> RequestCodec { get; }
    ITcpDataCodec<TResponse> ResponseCodec { get; }
    Task<TResponse> HandleRequestAsync(TRequest request);
}