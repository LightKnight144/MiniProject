using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();


app.Run();

public enum SortEnum
{
    NameAscending,
    NameDescending,
    LiterAscending,
    LiterDescending,
    PriceAscending,
    PriceDescending
}

public static class EnumerableExtensions
{
    public static List<Milk> Search(this List<Milk> milkshop, string? value)
    {
        if (value != null)
            return milkshop
                .FindAll(x => x.Name.StartsWith(value)
                || x.Liter.ToString().StartsWith(value)
                || x.Price.ToString().StartsWith(value));
        return milkshop;
    }
    public static List<Milk> Filter(this List<Milk> milkshop, string? name, int? liter, int? price)
    {
        List<Milk> buffer = milkshop;
        if (name != null)
        {
            buffer = buffer.FindAll(x => x.Name == name);
        }
        if (liter != null)
        {
            buffer = buffer.FindAll(x => x.Liter == liter);
        }
        if (price != null)
        {
            buffer = buffer.FindAll(x => x.Price == price);
        }
        return buffer;
    }
    public static List<Milk> Sort(this List<Milk> milkshop, SortEnum? sort)
    {
        return sort switch
        {
            SortEnum.NameAscending => milkshop.OrderBy(x => x.Name).ToList(),
            SortEnum.NameDescending => milkshop.OrderByDescending(x => x.Name).ToList(),
            SortEnum.LiterAscending => milkshop.OrderBy(x => x.Liter).ToList(),
            SortEnum.LiterDescending => milkshop.OrderByDescending(x => x.Liter).ToList(),
            SortEnum.PriceAscending => milkshop.OrderBy(x => x.Price).ToList(),
            SortEnum.PriceDescending => milkshop.OrderByDescending(x => x.Price).ToList(),
            _ => milkshop,
        };
    }

    public static List<Milk> Pagination(this List<Milk> milkshop, int? size, int? number)
    {
        if (size != null && number != null)
            return milkshop
                .Skip(size.Value * number.Value)
                .Take(size.Value).ToList();
        return milkshop;
    }

}

public record class MilkCreateDTO(string Name, int Liter, int Price, string Mail);
public record class MilkUpdateDTO(Guid Id, string Name, int Liter, int Price, string Mail);
public class Milk
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Liter { get; set; }
    public int Price { get; set; }
    public string Mail { get; set; }
}

public class MilkRepository
{
    private ApplicationContext db = new();

    public void Create(MilkCreateDTO dto)
    {
        db.MilkShop.Add(new Milk() { Name = dto.Name, Liter = dto.Liter, Price = dto.Price, Mail = dto.Mail });
        db.SaveChanges();
    }
    public Milk ReadById(Guid id)
    {
        return db.MilkShop.Find(id) ?? throw new Exception("Not found");
    }
    public void Update(MilkUpdateDTO dto)
    {
        var milk = db.MilkShop.Find(dto.Id);
        milk.Name = dto.Name;
        milk.Liter = dto.Liter;
        milk.Price = dto.Price;
        milk.Mail = dto.Mail;
        db.SaveChanges();
    }
    public void Delete(Guid id)
    {
        var milk = db.MilkShop.Find(id);
        db.MilkShop.Remove(milk);
        db.SaveChanges();
    }

    public List<Milk> Read(string? findValue, int? size, int? number, string? name, int? liter, int? price, SortEnum? sort)
    {
        return db.MilkShop.ToList()
            .Filter(name, liter, price)
            .Search(findValue)
            .Sort(sort)
            .Pagination(size, number);
    }
    public List<Milk> ReadAll()
    {
        return Read(null, null, null, null, null, null, null);
    }
}

public class ApplicationContext : DbContext
{
    public DbSet<Milk> MilkShop { get; set; }
    public ApplicationContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Milk.db");
    }
}