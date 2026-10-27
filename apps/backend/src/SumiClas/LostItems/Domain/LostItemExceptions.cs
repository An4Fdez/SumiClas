using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Domain;

public static class LostItemExceptions
{
    public sealed class NotFound() : DomainException("LostItem.NotFound", "No se ha encontrado el objeto.");

    public sealed class IdRequired()
        : DomainException("LostItem.IdRequired", "El identificador del objeto no puede estar vacío.");

    public sealed class MarkForbidden()
        : DomainException(
            "LostItem.MarkForbidden",
            "Solo el autor del objeto perdido puede cambiar su estado.");

    public sealed class ChangeForbidden()
        : DomainException(
            "LostItem.ChangeForbidden",
            "Solo el autor del objeto puede modificarlo o eliminarlo.");

    public sealed class InvalidScope()
        : DomainException("LostItem.InvalidScope", "El alcance del objeto no es válido.");

    public sealed class InvalidStatus()
        : DomainException("LostItem.InvalidStatus", "El estado del objeto no es válido.");

    public sealed class SchoolRequiredForSchoolScope()
        : DomainException(
            "LostItem.SchoolRequiredForSchoolScope",
            "Un objeto con alcance de colegio debe indicar el colegio.");

    public sealed class ClassRequiredForSchoolScope()
        : DomainException(
            "LostItem.ClassRequiredForSchoolScope",
            "Un objeto con alcance de colegio debe indicar la clase desde la que se publica.");

    public sealed class ClassRequiredForClassScope()
        : DomainException(
            "LostItem.ClassRequiredForClassScope",
            "Un objeto con alcance de clase debe indicar la clase.");

    public sealed class DescriptionRequired()
        : DomainException("LostItem.DescriptionRequired", "La descripción no puede estar vacía.");

    public sealed class PhotoPrivacyConfirmationRequired()
        : DomainException(
            "LostItem.PhotoPrivacyConfirmationRequired",
            "Antes de adjuntar una foto hay que confirmar que no muestra a ningún menor.");

    public sealed class ClaimNotFound()
        : DomainException("LostItem.ClaimNotFound", "No se ha encontrado la reclamación.");

    public sealed class ClaimIdRequired()
        : DomainException("LostItem.ClaimIdRequired", "El identificador de la reclamación no puede estar vacío.");

    public sealed class ClaimantRequired()
        : DomainException("LostItem.ClaimantRequired", "La reclamación debe tener quien la hace.");

    public sealed class ClaimMessageRequired()
        : DomainException("LostItem.ClaimMessageRequired", "Cuenta el detalle que identifica el objeto.");

    public sealed class ClaimClassRequired()
        : DomainException(
            "LostItem.ClaimClassRequired",
            "Solo se pueden reclamar los objetos publicados en una clase.");

    public sealed class ClaimOwnItemForbidden()
        : DomainException(
            "LostItem.ClaimOwnItemForbidden",
            "No puedes reclamar un objeto que has publicado tú.");

    public sealed class ClaimResolveForbidden()
        : DomainException(
            "LostItem.ClaimResolveForbidden",
            "Solo el autor del objeto puede aceptar o rechazar una reclamación.");

    public sealed class AlreadyRecovered()
        : DomainException("LostItem.AlreadyRecovered", "El objeto ya está marcado como recuperado.");

    public sealed class ClaimAlreadyExists()
        : DomainException(
            "LostItem.ClaimAlreadyExists",
            "Ya tienes una reclamación pendiente sobre este objeto.");

    public sealed class ClaimNotPending()
        : DomainException("LostItem.ClaimNotPending", "La reclamación ya estaba resuelta.");

    public sealed class AuthorRequired()
        : DomainException("LostItem.AuthorRequired", "El objeto debe tener un autor.");
}
