using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserRegister
{
    public sealed record Command(Guid Id, string FullName, string Email, string Password) : IRequest, ICommand;

    /// <summary>
    /// Nothing is hashed until the password rules and the email format pass. A race on the unique email index is still
    /// caught as a 409 by the unit of work.
    /// </summary>
    public sealed class Register(IUserRepository repository, IPasswordHasher hasher, IClock clock) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Password password = Password.Create(command.Password);

            Email email = Email.Create(command.Email);

            User user = User.Register(command.Id, command.FullName, email, hasher.Generate(password), clock);

            IReadOnlyCollection<User> existing = await repository.SearchAsync(new UserSpecification(email: email), ct);
            if (existing.Count > 0)
            {
                throw new UserExceptions.EmailAlreadyRegistered();
            }

            await repository.AddAsync(user, ct);
        }
    }

    public sealed class Handler(Register register) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => register.ExecuteAsync(command, ct);
    }
}
