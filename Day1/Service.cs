using Microsoft.EntityFrameworkCore;

record class ServiceUpdateDTO(string Name, string Specialty, string Task, int Experience, string Phone);
public class Service(int id, string name, string specialty, string task, int experience, string phone)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Specialty { get; set; } = specialty;
    public string Task { get; set; } = task;
    public int Experience { get; set; } = experience;
    public string Phone { get; set; } = phone;
}

public class ApplicationContext : DbContext
{
    public DbSet<Service> Services => Set<Service>();
    public ApplicationContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=People.db");
    }
}