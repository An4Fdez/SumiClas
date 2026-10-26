using SumiClas.Shared.Domain;

namespace SumiClas.Assignments.Domain;

public static class AssignmentExceptions
{
    public sealed class NotFound() : DomainException("Assignment.NotFound", "No se han encontrado los deberes.");

    public sealed class IdRequired()
        : DomainException("Assignment.IdRequired", "El identificador de los deberes no puede estar vacío.");

    public sealed class SubjectNotInClass()
        : DomainException(
            "Assignment.SubjectNotInClass",
            "La asignatura no pertenece a la clase de los deberes.");

    public sealed class ClassRequired()
        : DomainException("Assignment.ClassRequired", "Los deberes deben pertenecer a una clase.");

    public sealed class SubjectRequired()
        : DomainException("Assignment.SubjectRequired", "Los deberes deben tener una asignatura.");

    public sealed class TitleRequired()
        : DomainException("Assignment.TitleRequired", "El título de los deberes no puede estar vacío.");

    public sealed class AuthorRequired()
        : DomainException("Assignment.AuthorRequired", "Los deberes deben tener un autor.");

    public sealed class ChangeForbidden()
        : DomainException(
            "Assignment.ChangeForbidden",
            "Solo el autor de los deberes puede modificarlos o eliminarlos.");

    public sealed class CompletionUserRequired()
        : DomainException(
            "Assignment.CompletionUserRequired",
            "La marca de hecho requiere un usuario.");

    public sealed class CommentAuthorRequired()
        : DomainException("Assignment.CommentAuthorRequired", "El comentario debe tener un autor.");

    public sealed class CommentTextRequired()
        : DomainException("Assignment.CommentTextRequired", "El comentario no puede estar vacío.");
}
