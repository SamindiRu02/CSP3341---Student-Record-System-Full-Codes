namespace StudentRecordSystem;


//Stores student details and manages course enrolments.
public sealed class Student
{
    private readonly List<Enrolment> _enrolments = new();

    public string StudentId { get; } //Gets the unique stident ID
    public string FullName { get; } //Gets the student full name
    public string Status { get; } //Gets the student's current status

    //Gets enrolment records
    public IReadOnlyList<Enrolment> Enrolments =>
        _enrolments.AsReadOnly();

    public Student(string studentId, string fullName, string status)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Student name is required.");

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Student status is required.");

        StudentId = studentId.Trim();
        FullName = fullName.Trim();
        Status = status.Trim();
    }

    //Adds a course enrolment and prevents duplicate courses
    public void AddEnrolment(Enrolment enrolment)
    {
        ArgumentNullException.ThrowIfNull(enrolment);

        if (_enrolments.Any(e =>
            e.Course.Code.Equals(
                enrolment.Course.Code,
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "The student is already enrolled in this course.");
        }

        _enrolments.Add(enrolment);
    }

    public bool RemoveEnrolment(string courseCode)
    {
        if (string.IsNullOrWhiteSpace(courseCode))
            return false;

        return _enrolments.RemoveAll(e =>
            e.Course.Code.Equals(
                courseCode.Trim(),
                StringComparison.Ordinal)) > 0;
    }


    // Calculate the weighted average mark for the student
    public decimal CalculateWAM()
    {
        if (_enrolments.Count == 0)
            throw new InvalidOperationException(
                "Add an enrolment before calculating WAM.");

        decimal totalCredits = _enrolments.Sum(
            e => (decimal)e.Course.Credits);

        decimal weightedTotal = _enrolments.Sum(
            e => e.Mark * e.Course.Credits);

        return weightedTotal / totalCredits;
    }
}