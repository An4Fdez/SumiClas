using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserNameChange
{
    public sealed record Command(string FullName) : IRequest, ICommand;

    public sealed class Change(IUserRepository repository, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid userId = userProvider.GetCurrentUserId();

            User user = await repository.GetByIdAsync(userId, ct)
                ?? throw new UserExceptions.NotFound();

            user.Rename(command.FullName);
        }
    }

    public sealed class Handler(Change change) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => change.ExecuteAsync(command, ct);
    }
}
