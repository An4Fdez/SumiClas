using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Domain;

public static class CalendarEventExceptions
{
    public sealed class NotFound() : DomainException("CalendarEvent.NotFound", "No se ha encontrado el evento.");

    public sealed class IdRequired()
        : DomainException("CalendarEvent.IdRequired", "El identificador del evento no puede estar vacío.");

    public sealed class InvalidScope()
        : DomainException("CalendarEvent.InvalidScope", "El alcance del evento no es válido.");

    public sealed class InvalidType()
        : DomainException("CalendarEvent.InvalidType", "El tipo del evento no es válido.");

    public sealed class SchoolRequiredForSchoolScope()
        : DomainException(
            "CalendarEvent.SchoolRequiredForSchoolScope",
            "Un evento con alcance de colegio debe indicar el colegio.");

    public sealed class ClassRequiredForSchoolScope()
        : DomainException(
            "CalendarEvent.ClassRequiredForSchoolScope",
            "Un evento con alcance de colegio debe indicar la clase desde la que se publica.");

    public sealed class ClassRequiredForClassScope()
        : DomainException(
            "CalendarEvent.ClassRequiredForClassScope",
            "Un evento con alcance de clase debe indicar la clase.");

    public sealed class TitleRequired()
        : DomainException("CalendarEvent.TitleRequired", "El título del evento no puede estar vacío.");

    public sealed class AuthorRequired()
        : DomainException("CalendarEvent.AuthorRequired", "El evento debe tener un autor.");

    public sealed class StudentRequiredForBirthday()
        : DomainException("CalendarEvent.StudentRequiredForBirthday", "Un cumpleaños debe indicar el alumno.");

    public sealed class UseCreateBirthdayForBirthdays()
        : DomainException(
            "CalendarEvent.UseCreateBirthdayForBirthdays",
            "Un cumpleaños no se puede crear como un evento normal. "
            + "Usa la opción de añadir el cumpleaños del alumno.");

    public sealed class ChangeForbidden()
        : DomainException(
            "CalendarEvent.ChangeForbidden",
            "Solo el autor del evento puede modificarlo o eliminarlo.");

    public sealed class BirthdayChangeInvalid()
        : DomainException(
            "CalendarEvent.BirthdayChangeInvalid",
            "Un cumpleaños no se puede modificar. Elimínalo y añade el cumpleaños de nuevo.");

    public sealed class InvalidMonth()
        : DomainException("CalendarEvent.InvalidMonth", $"El mes debe estar entre {EventDate.FirstMonth} y {EventDate.LastMonth}.");

    public sealed class InvalidDay()
        : DomainException("CalendarEvent.InvalidDay", "El día no es válido para ese mes.");
}
