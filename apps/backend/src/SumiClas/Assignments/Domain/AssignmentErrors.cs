using SumiClas.Shared.Domain;

namespace SumiClas.Assignments.Domain;

public static class AssignmentErrors
{
    public static readonly Error IdRequired = new("Assignment.IdRequired", "El identificador de los deberes no puede estar vacío.");
    public static readonly Error NotFound = new("Assignment.NotFound", "No se han encontrado los deberes.");
    public static readonly Error SubjectNotInClass = new("Assignment.SubjectNotInClass", "La asignatura no pertenece a la clase de los deberes.");
    public static readonly Error ClassRequired = new("Assignment.ClassRequired", "Los deberes deben pertenecer a una clase.");
    public static readonly Error SubjectRequired = new("Assignment.SubjectRequired", "Los deberes deben tener una asignatura.");
    public static readonly Error TitleRequired = new("Assignment.TitleRequired", "El título de los deberes no puede estar vacío.");
    public static readonly Error AuthorRequired = new("Assignment.AuthorRequired", "Los deberes deben tener un autor.");
    public static readonly Error CommentAuthorRequired = new("Assignment.CommentAuthorRequired", "El comentario debe tener un autor.");
    public static readonly Error CommentTextRequired = new("Assignment.CommentTextRequired", "El comentario no puede estar vacío.");
}
