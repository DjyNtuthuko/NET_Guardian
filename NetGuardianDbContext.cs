using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Reflection.Emit;

namespace NET_Guardian
{
    public class NetGuardianDbContext : DbContext
    {
        public const string ConnectionString = "server=localhost;database=net_guardian_db;user=root;password=;";

        public DbSet<GuardianTask> GuardianTasks { get; set; }
        public DbSet<ActivityLogEntry> ActivityLogEntries { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(
                    ConnectionString,
                    new MySqlServerVersion(new Version(8, 0, 0)));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuardianTask>().Property(task => task.Title).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<GuardianTask>().Property(task => task.Priority).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<ActivityLogEntry>().Property(entry => entry.Action).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<QuizAttempt>().Property(attempt => attempt.ResultMessage).IsRequired().HasMaxLength(200);
        }
    }

    [DbContext(typeof(NetGuardianDbContext))]
    [Migration("202606210001_InitialCreate")]
    public class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `GuardianTasks` (
                    `GuardianTaskId` int NOT NULL AUTO_INCREMENT,
                    `Title` varchar(150) NOT NULL,
                    `Description` longtext NOT NULL,
                    `Priority` varchar(20) NOT NULL,
                    `ReminderDate` datetime(6) NULL,
                    `IsCompleted` tinyint(1) NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    CONSTRAINT `PK_GuardianTasks` PRIMARY KEY (`GuardianTaskId`)
                ) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `ActivityLogEntries` (
                    `ActivityLogEntryId` int NOT NULL AUTO_INCREMENT,
                    `Action` varchar(100) NOT NULL,
                    `Details` longtext NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    CONSTRAINT `PK_ActivityLogEntries` PRIMARY KEY (`ActivityLogEntryId`)
                ) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `QuizAttempts` (
                    `QuizAttemptId` int NOT NULL AUTO_INCREMENT,
                    `Score` int NOT NULL,
                    `TotalQuestions` int NOT NULL,
                    `ResultMessage` varchar(200) NOT NULL,
                    `CompletedAt` datetime(6) NOT NULL,
                    CONSTRAINT `PK_QuizAttempts` PRIMARY KEY (`QuizAttemptId`)
                ) CHARACTER SET=utf8mb4;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ActivityLogEntries");
            migrationBuilder.DropTable(name: "GuardianTasks");
            migrationBuilder.DropTable(name: "QuizAttempts");
        }
    }
}