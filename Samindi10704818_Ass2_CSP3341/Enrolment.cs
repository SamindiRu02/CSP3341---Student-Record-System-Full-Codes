namespace StudentRecordSystem;

public sealed class Enrolment
{
    public Course Course { get; }
    public decimal Mark { get; }

    // Determine the grade absed on the student's mark
    public string Grade =>  
        Mark >= 80 ? "HD" :
        Mark >= 70 ? "D" :
        Mark >= 60 ? "CR" :
        Mark >= 50 ? "P" : "F";

    public Enrolment(Course course, decimal mark)
    {
        Course = course ??
            throw new ArgumentNullException(nameof(course));

        if (mark < 0m || mark > 100m)
            throw new ArgumentOutOfRangeException(
                nameof(mark), "Mark must be between 0 and 100.");

        Mark = mark;
    }
}