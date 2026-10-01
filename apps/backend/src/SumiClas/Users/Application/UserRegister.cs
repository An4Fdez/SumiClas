using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserRegister
{
    public sealed record Command(Guid Id, string FullName, string Email) : IRequest<Result>, ICommand;

    public sealed class UseCase(IUserRepository users)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var email = Email.Create(command.Email);
            if (email.IsFailure)
            {
                return Result.Failure(email.Error!);
            }

            var registration = User.Register(command.Id, command.FullName, email.Value);
            if (registration.IsFailure)
            {
                return Result.Failure(registration.Error!);
            }

            var existing = await users.SearchAsync(new UserSpecification(email: email.Value), ct);
            if (existing.Count > 0)
            {
                return Result.Failure(UserErrors.EmailAlreadyRegistered);
            }

            await users.AddAsync(registration.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
