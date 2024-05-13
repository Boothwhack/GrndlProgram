namespace Application;

public class DatabaseParseException(string message) : Exception(message);

public struct Person
{
    public int ID;
    public string Name;
    public DateOnly DateOfBirth;
}

public class PersonDatabase(Dictionary<int, Person> persons, int nextId)
{
    public PersonDatabase() : this(new Dictionary<int, Person>(), 0)
    {
    }

    public IEnumerable<Person> Persons => persons.Values;

    public int Count => persons.Count;

    public Person? this[int id] => persons[id];

    public Person CreatePerson(string name, DateOnly dateOfBirth)
    {
        var id = nextId++;
        return persons[id] = new Person { ID = id, Name = name, DateOfBirth = dateOfBirth };
    }

    public void DeletePerson(int id)
    {
        persons.Remove(id);
    }

    private static Person ParsePerson(string line)
    {
        var parts = line.Split(";");
        if (parts.Length != 3) throw new DatabaseParseException("Invalid person line format.");

        if (!int.TryParse(parts[0], out var id))
            throw new DatabaseParseException("Invalid ID field format.");
        var name = parts[1].Replace("%3B", ";");
        if (!int.TryParse(parts[2], out var dateOfBirthDayNumber))
            throw new DatabaseParseException("Invalid date of birth format.");

        var dateOfBirth = DateOnly.FromDayNumber(dateOfBirthDayNumber);

        return new Person { ID = id, Name = name, DateOfBirth = dateOfBirth };
    }

    private static void WritePerson(Person person, TextWriter writer)
    {
        writer.Write(person.ID);
        writer.Write(';');
        var escapedName = person.Name.Replace(";", "%3B");
        writer.Write(escapedName);
        writer.Write(';');
        writer.Write(person.DateOfBirth.DayNumber);
    }

    public static PersonDatabase ReadFrom(TextReader reader)
    {
        Dictionary<int, Person> persons = new();
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
        {
            var person = ParsePerson(line);
            persons[person.ID] = person;
        }

        var nextId = persons.Count > 0
            ? persons.Values.MaxBy(person => person.ID).ID + 1
            : 0;

        return new PersonDatabase(persons, nextId);
    }

    public static void WriteTo(PersonDatabase database, TextWriter writer)
    {
        foreach (var person in database.Persons)
        {
            WritePerson(person, writer);
            writer.WriteLine();
        }
    }
}