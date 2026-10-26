using SumiClas.Shared.Domain;

namespace SumiClas.ClassExpenses.Domain;

public static class ClassExpenseExceptions
{
    public sealed class NotFound() : DomainException("ClassExpense.NotFound", "No se ha encontrado el gasto.");

    public sealed class IdRequired()
        : DomainException("ClassExpense.IdRequired", "El identificador del gasto no puede estar vacío.");

    public sealed class ParticipantNotInClass()
        : DomainException(
            "ClassExpense.ParticipantNotInClass",
            "Alguno de los participantes no pertenece a la clase.");

    public sealed class ParticipantNotFound()
        : DomainException("ClassExpense.ParticipantNotFound", "Alguno de los participantes no existe.");

    public sealed class ClassRequired()
        : DomainException("ClassExpense.ClassRequired", "El gasto debe pertenecer a una clase.");

    public sealed class ItemRequired()
        : DomainException("ClassExpense.ItemRequired", "El concepto del gasto no puede estar vacío.");

    public sealed class TotalAmountMustBePositive()
        : DomainException(
            "ClassExpense.TotalAmountMustBePositive",
            "El importe total debe ser mayor que cero.");

    public sealed class AtLeastOneParticipantRequired()
        : DomainException(
            "ClassExpense.AtLeastOneParticipantRequired",
            "El gasto debe tener al menos un participante.");

    public sealed class AuthorRequired()
        : DomainException("ClassExpense.AuthorRequired", "El gasto debe tener un autor.");

    public sealed class ChangeForbidden()
        : DomainException(
            "ClassExpense.ChangeForbidden",
            "Solo el autor del gasto puede modificarlo o eliminarlo.");

    public sealed class NotAParticipant()
        : DomainException("ClassExpense.NotAParticipant", "Este usuario no participa en el gasto.");

    public sealed class HasPayments()
        : DomainException(
            "ClassExpense.HasPayments",
            "No se puede eliminar el gasto porque alguna familia ya ha pagado.");
}
