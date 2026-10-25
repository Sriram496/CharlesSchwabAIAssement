using CourseRegistrationApplication.Controllers;
using CourseRegistrationApplication.Eligibility;
using CourseRegistrationApplication.Services;
using CourseRegistrationApplication.Storage;
using CourseRegistrationApplication.Waitlist;

var offering = new Offering("CS101", 5);
var store = new InMemoryStore(offering);
var eligibility = new EligibilityProvider();
var learners = new (string Id, bool PrerequisiteMet)[]
{
    ("L1", true), ("L2", true), ("L3", true), ("L4", false), ("L5", true),
    ("L6", true), ("L7", true), ("L8", true), ("L9", false), ("L10", true),
};

foreach (var (id, prerequisiteMet) in learners)
{
    eligibility.AddLearner(id, prerequisiteMet);
}

var waitlist = new WaitlistQueueManager(store, eligibility);
ICourseOfferingService service = new CourseOfferingService(store, waitlist, eligibility, TimeProvider.System);
var controller = new EnrollmentController(service);

Console.WriteLine($"Course offering {offering.OfferingId} (capacity {offering.Capacity}). Commands: enroll <id>, drop <id>, status <id>, quit");
Console.WriteLine($"Note: learners {learners[0].Id} to {learners[^1].Id} are available.");

while (true)
{
    Console.Write("> ");
    string? line = Console.ReadLine();
    if (line is null || line.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    Console.WriteLine(controller.Handle(line));
}
