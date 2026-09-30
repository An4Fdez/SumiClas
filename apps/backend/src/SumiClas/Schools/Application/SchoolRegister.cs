using MediatR;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Application;

public static class SchoolRegister
{
    public sealed record Command(Guid Id, string Name, string OfficialCode, string? Address)
        : IRequest<Result>, ICommand;

    public sealed class UseCase(ISchoolRepository schools)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var registration = School.Register(command.Id, command.Name, command.OfficialCode, command.Address);
            if (registration.IsFailure)
            {
                return Result.Failure(registration.Error!);
            }

            await schools.AddAsync(registration.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
