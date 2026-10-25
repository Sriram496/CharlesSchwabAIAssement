namespace CourseRegistrationApplication.Storage;

public sealed record EnrollmentRecord(string LearnerId, DateTimeOffset EnrolledAtUtc);
