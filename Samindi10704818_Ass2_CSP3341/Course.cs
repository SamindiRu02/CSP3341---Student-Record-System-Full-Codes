
namespace StudentRecordSystem;

// Represent a course and stores values for the name, code, and credit value.
public sealed class Course
{
    public string Code { get; }
    public string Name { get; }
    public int Credits { get; }

    public Course(string code, string name, int credits)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Course code is required.");

        if (code.Any(char.IsWhiteSpace))
            throw new ArgumentException(
                "Course code cannot contain spaces.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name is required.");

        if (credits <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(credits), "Credits must be positive.");

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Credits = credits;
    }

    public override string ToString()
    {
        return $"{Code} - {Name} ({Credits} credits)";
    }
}
