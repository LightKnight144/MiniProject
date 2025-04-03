using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

string posterRest = "poster";
string pattern = @"^\+?\d{1,3}[-.\s]?\(?\d{1,4}?\)?\d{1,4}\d{1,9}$";

app.MapGet(posterRest, async (int? id) =>
{
    using ApplicationContext db = new();
    if (id != null)
    {
        Poster? poster = await db.Posters.FirstOrDefaultAsync(x => x.Id == id);
        if (poster == null)
        {
            return Results.NotFound(new { message = "Фильм не найден" });
        }
        else
        {
            return Results.Json(poster);
        }
    }
    else
    {
        return Results.Json(await db.Posters.ToListAsync());
    }
});
app.MapPost(posterRest, (Poster p) =>
{
    using ApplicationContext db = new();
    if (Regex.IsMatch(p.Phone, pattern))
    {
        if (p.Price < 1 || p.Rating != "0" && p.Rating != "6" && p.Rating != "12" && p.Rating != "16" && p.Rating != "18")
        {
            return "Данные указаны неверно";
        }
        else
        {
            db.Posters.Add(p);
            db.SaveChanges();
            return "Данные добавлены";
        }
    }
    else
    {
        return "Указан неверно телефон";
    }
});
app.MapPut(posterRest, async (PosterUpdateDTO dto, int id) =>
{
    using ApplicationContext db = new();
    var change = await db.Posters.FirstOrDefaultAsync(x => x.Id == id);
    if (change == null)
    {
        return Results.NotFound(new { message = "Фильм не найден" });
    }
    else
    {
        if (dto.Name != change.Name && dto.Name != "")
        {
            change.Name = dto.Name;
        }
        if (dto.Number != change.Number && dto.Number != null)
        {
            change.Number = dto.Number;
        }
        if (dto.Price != change.Price && dto.Price != null && dto.Price > 1)
        {
            change.Price = dto.Price;
        }
        if (dto.Places != change.Places && dto.Places != null)
        {
            change.Places = dto.Places;
        }
        if (dto.Rating != change.Rating && dto.Rating != "" && dto.Rating == "0" || dto.Rating == "6" || dto.Rating == "12" || dto.Rating == "16" || dto.Rating == "18")
        {
            change.Rating = dto.Rating;
        }
        if (dto.Creation != change.Creation)
        {
            change.Creation = dto.Creation;
        }
        if (dto.Show != change.Show && dto.Show != null)
        {
            change.Show = dto.Show;
        }
        if (dto.Phone != change.Phone && dto.Phone != "" && Regex.IsMatch(dto.Phone, pattern))
        {
            change.Phone = dto.Phone;
        }
        await db.SaveChangesAsync();
        return Results.Json(change);
    }
});
app.MapDelete(posterRest, async (int id) =>
{
    using ApplicationContext db = new();
    Poster? poster = await db.Posters.FirstOrDefaultAsync(x => x.Id == id);
    if (poster == null)
    {
        return Results.NotFound(new { message = "Фильм не найден" });
    }
    else
    {
        db.Posters.Remove(poster);
        await db.SaveChangesAsync();
        return Results.Json(poster);
    }
});

app.Run();
record class PosterUpdateDTO(int Number, string Name, int Price, int Places, string Rating, DateOnly Creation, int Show, string Phone);
public class Poster(int id, int number, string name, int price, int places, string rating, DateOnly creation, int show, string phone)
{
    public int Id { get; set; } = id;
    public int Number { get; set; } = number;
    public string Name { get; set; } = name;
    public int Price { get; set; } = price;
    public int Places { get; set; } = places;
    public string Rating { get; set; } = rating;
    public DateOnly Creation { get; set; } = creation;
    public int Show { get; set; } = show;
    public string Phone { get; set; } = phone;
};

public class ApplicationContext : DbContext
{
    public DbSet<Poster> Posters => Set<Poster>();
    public ApplicationContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Poster.db");
    }
}