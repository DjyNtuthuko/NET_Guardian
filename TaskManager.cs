using System;
using System.Collections.Generic;
using System.Linq;

namespace NET_Guardian
{
    public class TaskManager
    {
        // Saves a new task
        public GuardianTask AddTask(string title, string description, string priority, DateTime? reminderDate)
        {
            GuardianTask task = new GuardianTask
            {
                Title = title,
                Description = description,
                Priority = priority,
                ReminderDate = reminderDate,
                IsCompleted = false,
                CreatedAt = DateTime.Now
            };

            using NetGuardianDbContext database = new NetGuardianDbContext();
            database.GuardianTasks.Add(task);
            database.SaveChanges();
            return task;
        }

        // Loads tasks from the database
        public List<GuardianTask> GetTasks()
        {
            using NetGuardianDbContext database = new NetGuardianDbContext();
            return database.GuardianTasks
                .OrderBy(task => task.IsCompleted)
                .ThenByDescending(task => task.CreatedAt)
                .ToList();
        }

        public List<GuardianTask> GetDueReminders()
        {
            DateTime today = DateTime.Today;
            using NetGuardianDbContext database = new NetGuardianDbContext();
            return database.GuardianTasks
                .Where(task => !task.IsCompleted && task.ReminderDate.HasValue && task.ReminderDate.Value.Date <= today)
                .OrderBy(task => task.ReminderDate)
                .ToList();
        }

        public void CompleteTask(int taskId)
        {
            using NetGuardianDbContext database = new NetGuardianDbContext();
            GuardianTask? task = database.GuardianTasks.Find(taskId);
            if (task == null)
                throw new InvalidOperationException("The selected task could not be found.");

            task.IsCompleted = true;
            database.SaveChanges();
        }

        public void DeleteTask(int taskId)
        {
            using NetGuardianDbContext database = new NetGuardianDbContext();
            GuardianTask? task = database.GuardianTasks.Find(taskId);
            if (task == null)
                throw new InvalidOperationException("The selected task could not be found.");

            database.GuardianTasks.Remove(task);
            database.SaveChanges();
            
        }
    }
}