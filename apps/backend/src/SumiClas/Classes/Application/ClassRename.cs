using MediatR;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Classes.Application;

public static class ClassRename
{
    public sealed record Command(Guid ClassId, string Name) : IRequest, ICommand;

    public sealed class Rename(IClassRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var schoolClass = await repository.GetByIdAsync(command.ClassId, ct)
                ?? throw new ClassExceptions.NotFound();

            var current = userProvider.GetCurrentUserId().OrThrow();

            (await queryBus.Send(new UserClassRepresentativeEnsure.Query(current, command.ClassId), ct)).OrThrow();

            (await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct)).OrThrow();

            schoolClass.Rename(command.Name);
        }
    }

    public sealed class Handler(Rename rename) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => rename.ExecuteAsync(command, ct);
    }
}
