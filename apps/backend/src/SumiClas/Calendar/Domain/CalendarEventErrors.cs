using SumiClas.Shared.Domain;

namespace SumiClas.Calendar.Domain;

public static class CalendarEventErrors
{
    public static readonly Error ClassRequiredForClassScope = new("CalendarEvent.ClassRequiredForClassScope", "Un evento con alcance de clase debe indicar la clase.");
    public static readonly Error TitleRequired = new("CalendarEvent.TitleRequired", "El título del evento no puede estar vacío.");
    public static readonly Error AuthorRequired = new("CalendarEvent.AuthorRequired", "El evento debe tener un autor.");
    public static readonly Error StudentRequiredForBirthday = new("CalendarEvent.StudentRequiredForBirthday", "Un cumpleaños debe indicar el alumno.");
    public static readonly Error UseCreateBirthdayForBirthdays = new("CalendarEvent.UseCreateBirthdayForBirthdays", "Los cumpleaños se crean con CreateBirthday, no con Create.");
    public static readonly Error InvalidMonth = new("CalendarEvent.InvalidMonth", "El mes debe estar entre 1 y 12.");
    public static readonly Error InvalidDay = new("CalendarEvent.InvalidDay", "El día no es válido para ese mes.");
}
