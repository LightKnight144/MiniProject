using Microsoft.EntityFrameworkCore;

record class RestaurantUpdateDTO(string Name, int Rank, string Street, DateOnly Creation, string Mail);
public class Restaurant(int id, string name, int rank, string street, DateOnly creation, string mail)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public int Rank { get; set; } = rank;
    public string Street { get; set; } = street;
    public DateOnly Creation { get; set; } = creation;
    public string Mail { get; set; } = mail;
}

public class ApplicationContext : DbContext
{
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public ApplicationContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Restaurant.db");
    }
}