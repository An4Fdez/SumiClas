using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserRegister
{
    public sealed record Command(Guid Id, string FullName, string Email, string Password) : IRequest<Result>, ICommand;

    /// <summary>
    /// Nothing is hashed until the password rules and the email format pass. A race on the unique email index is still
    /// caught as a 409 by the unit of work.
    /// </summary>
    public sealed class Register(IUserRepository users, IPasswordHasher hasher, IClock clock) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var password = Password.Create(command.Password);
            if (password.IsFailure)
            {
                return Result.Failure(password.Error!);
            }

            var email = Email.Create(command.Email);
            if (email.IsFailure)
            {
                return Result.Failure(email.Error!);
            }

            var registration = User.Register(
                command.Id, command.FullName, email.Value, hasher.Generate(password.Value), clock);
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

    public sealed class Handler(Register register) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => register.ExecuteAsync(command, ct);
    }
}
