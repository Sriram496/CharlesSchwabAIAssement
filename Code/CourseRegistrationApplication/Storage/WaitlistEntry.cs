namespace CourseRegistrationApplication.Storage;

public sealed record WaitlistEntry(string LearnerId, long Sequence, DateTimeOffset JoinedAtUtc);
