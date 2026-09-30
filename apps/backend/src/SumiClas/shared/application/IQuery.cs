using MediatR;

namespace SumiClas.Shared.Application;

public interface IQuery<out TResponse> : IRequest<TResponse>;
