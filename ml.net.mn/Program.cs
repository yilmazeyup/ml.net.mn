using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;

namespace PersonnelScheduleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize ML.NET context
            var mlContext = new MLContext();

            // Sample JSON data
            string jsonData = @"
                [
                    {
                        ""id"": 6,
                        ""storelabel"": ""testStoreLabel"",
                        ""openingtime"": ""10:00:00Z"",
                        ""closingtime"": ""21:00:00Z"",
                        ""personnelname"": ""testPersonnelName"",
                        ""username"": ""testUserName"",
                        ""income"": 684.10,
                        ""invoicecount"": 18,
                        ""workinghours"": 13.65,
                        ""expense"": 191.10,
                        ""days"": 6,
                        ""starttime"": ""10:00:00Z"",
                        ""endtime"": ""15:00:00Z"",
                        ""simultaneouspersonnel"": 2
                    }
                ]";

            // Deserialize JSON data to PersonnelRecord objects
            var personnelQuery = JsonConvert.DeserializeObject<List<PersonnelRecord>>(jsonData);

            // Convert query results to personnel data (mocking database retrieval)
            var personnelData = ConvertQueryResult(personnelQuery);

            // Prepare training data for the model
            var trainingData = mlContext.Data.LoadFromEnumerable(personnelData);

            // Define feature and label columns
            var featureColumns = new[] { "Income", "InvoiceCount", "WorkingHours", "Expense" };
            var labelColumn = "Score";

            // Create a machine learning pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", featureColumns)
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: labelColumn, featureColumnName: "Features"));

            // Train the model
            var model = pipeline.Fit(trainingData);

            // Make predictions
            var predictions = model.Transform(trainingData);
            var scoredPersonnel = mlContext.Data.CreateEnumerable<ScoredPersonnelRecord>(predictions, reuseRowObject: false);

            // Create daily personnel selection and schedule
            var schedule = CreateScheduleHub(scoredPersonnel, personnelData);

            // Print the schedule to the console
            foreach (var work in schedule)
            {
                Console.WriteLine($"Staff: {work.UserName}, Work Day: {work.WorkDay}, Start Hour: {work.StartHour}, End Hour: {work.EndHour}, Date: {work.Date}");
            }
        }

        public static List<ScheduledWork> CreateScheduleHub(IEnumerable<ScoredPersonnelRecord> scoredRecords, IEnumerable<PersonnelRecord> personnelData)
        {
            // Initialize a list to hold the scheduled work.
            var schedule = new List<ScheduledWork>();

            // Create a dictionary to group personnel records by day.
            var dayWisePersonnel = new Dictionary<int, List<ScoredPersonnelRecord>>();

            // Group the scored records by days.
            foreach (var record in scoredRecords)
            {
                if (!dayWisePersonnel.ContainsKey(record.Days))
                {
                    dayWisePersonnel[record.Days] = new List<ScoredPersonnelRecord>();
                }
                dayWisePersonnel[record.Days].Add(record);
            }

            // Iterate over each day to create the schedule.
            foreach (var day in dayWisePersonnel.Keys)
            {
                // Order personnel by their score for each day.
                var dayPersonnel = dayWisePersonnel[day].OrderByDescending(p => p.Score).ToList();

                // Retrieve store opening and closing times.
                TimeSpan storeOpen = personnelData.First().OpeningTime;
                TimeSpan storeClose = personnelData.First().ClosingTime;

                // Schedule personnel for the day.
                foreach (var personnel in dayPersonnel)
                {
                    // Schedule personnel only if their end time is after the store opening time.
                    if (storeOpen < personnel.EndTime)
                    {
                        // Determine the end of the shift.
                        TimeSpan shiftEnd = storeClose < personnel.EndTime ? storeClose : personnel.EndTime;

                        // Add the scheduled work to the list.
                        schedule.Add(new ScheduledWork
                        {
                            UserName = personnel.UserName,
                            WorkDay = day.ToString(),
                            StartHour = storeOpen,
                            EndHour = shiftEnd
                        });

                        // Update the store opening time for the next personnel.
                        storeOpen = shiftEnd;
                    }

                    // Break the loop if the store's closing time is reached.
                    if (storeOpen >= storeClose) break;
                }
            }

            return schedule;
        }

        public static ScoredPersonnelRecord SelectPersonnelForDay(IEnumerable<ScoredPersonnelRecord> scoredRecords, int day, IEnumerable<PersonnelRecord> personnelData)
        {
            // Filter personnel based on the specified day and their suitable working hours.
            // The criteria include:
            // - Personnel should be working on the specified day.
            // - Their working hours should not exceed half of the store's open hours.
            // - Their start and end times should be within the store's operating hours.
            var filteredPersonnel = scoredRecords
                .Where(p => p.Days == day &&
                            p.WorkingHours <= (p.ClosingTime - p.OpeningTime).TotalHours / 2 &&
                            p.StartTime >= p.OpeningTime && p.EndTime <= p.ClosingTime)
                .ToList();

            // Select the personnel with the highest score from the filtered list.
            // If there are no personnel meeting the criteria, it returns null.
            return filteredPersonnel.OrderByDescending(p => p.Score).FirstOrDefault();
        }

        public static IEnumerable<PersonnelRecord> ConvertQueryResult(IEnumerable<dynamic> queryResult)
        {
            // Initialize a list to hold converted PersonnelRecord objects.
            var resultList = new List<PersonnelRecord>();

            // Iterate through each dynamic item in the query result.
            foreach (dynamic item in queryResult)
            {
                // Create a new PersonnelRecord object and populate it with data from the dynamic item.
                // This includes converting the dynamic types to the appropriate types used in PersonnelRecord.
                var record = new PersonnelRecord
                {
                    StoreLabel = item.storelabel,
                    UserName = item.username,
                    Income = Convert.ToSingle(item.income),
                    InvoiceCount = Convert.ToSingle(item.invoicecount),
                    WorkingHours = Convert.ToSingle(item.workinghours),
                    Expense = Convert.ToSingle(item.expense),
                    Days = Convert.ToInt32(item.days),
                    OpeningTime = TimeSpan.Parse(item.openingtime), // Convert to TimeSpan
                    ClosingTime = TimeSpan.Parse(item.closingtime), // Convert to TimeSpan
                    StartTime = TimeSpan.Parse(item.starttime), // Convert to TimeSpan
                    EndTime = TimeSpan.Parse(item.endtime), // Convert to TimeSpan
                    SimultaneousPersonnel = Convert.ToInt32(item.simultaneouspersonnel),
                    Label = item.days.ToString()
                };

                // Add the converted record to the result list.
                resultList.Add(record);
            }

            // Return the list of converted PersonnelRecord objects.
            return resultList;
        }

        public DateTime GetFirstDayOfNextMonth()
        {
            // Get today's date.
            var today = DateTime.Today;

            // Create a new DateTime representing the first day of the current month and add one month to it.
            // This results in the first day of the next month.
            var firstDayNextMonth = new DateTime(today.Year, today.Month, 1).AddMonths(1);

            // Return the calculated date.
            return firstDayNextMonth;
        }

        public int GetDayOfWeekAsInt(DateTime date)
        {
            // Convert the day of the week for the given date to an integer.
            // The conversion is based on the DayOfWeek enumeration where
            // Sunday = 0, Monday = 1, and so on up to Saturday = 6.
            return (int)date.DayOfWeek;
        }


        public class PersonnelRecord
        {
            public string StoreLabel { get; set; }
            public string UserName { get; set; }
            public float Income { get; set; }
            public float InvoiceCount { get; set; }
            public float WorkingHours { get; set; }
            public float Expense { get; set; }
            public int Days { get; set; }
            public TimeSpan OpeningTime { get; set; }
            public TimeSpan ClosingTime { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int SimultaneousPersonnel { get; set; }
            public string Label { get; set; }
            public float Score { get; set; }
            // Ek olarak ihtiyaç duyduğunuz diğer özellikler buraya eklenebilir.
        }

        public class ScoredPersonnelRecord : PersonnelRecord
        {
            public float Score { get; set; }
            // İhtiyaç duyduğunuz ekstra özellikler buraya eklenebilir, ancak şu anda görünüşe göre yalnızca 'Score' özelliğine ihtiyaç var.
        }

        public class ScheduledWork
        {
            public string UserName { get; set; }
            public string WorkDay { get; set; }
            public TimeSpan StartHour { get; set; }
            public TimeSpan EndHour { get; set; }
            public DateTime Date { get; set; }  // Eklendi
        }

    }
}
