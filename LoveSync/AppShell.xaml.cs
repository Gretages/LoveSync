using LoveSync.ViewModels;

namespace LoveSync
{
    public partial class AppShell : Shell
    {
        public AppShell(
            NotesViewModel notesVm,
            CalendarViewModel calendarVm,
            BucketViewModel bucketVm,
            MatchesViewModel matchesVm)
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SwipePage), typeof(SwipePage));

            Task.Run(async () =>
            {
                await Task.Delay(2000);

                var tasks = new List<Task>
                {
                    notesVm.InitializeAsync(),
                    calendarVm.InitializeAsync(),
                    bucketVm.InitializeAsync(),
                    matchesVm.InitializeAsync()
                };

                await Task.WhenAll(tasks);

                System.Diagnostics.Debug.WriteLine("HÁTTÉRBETÖLTÉS KÉSZ: Minden adat letöltve!");
            });
        }
    }
}