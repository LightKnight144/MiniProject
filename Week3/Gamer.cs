using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

string gamerRest = "gamer";
string pattern = @"^\+?\d{1,3}[-.\s]?\(?\d{1,4}?\)?\d{1,4}\d{1,9}$";

List<Gamer> gamers = new List<Gamer>();
{
    using ApplicationContext db = new();
    foreach (var gamer in db.Gamers)
        gamers.Add(gamer);
}
List<Gamer> Find(string value)
{
    return gamers.FindAll(gamer =>
    gamer.Name.Contains(value) ||
    gamer.Level.ToString().Contains(value) ||
    gamer.Score.ToString().Contains(value) ||
    gamer.Hours.ToString().Contains(value));
}

List<Gamer> Pagination(int size, int number)
{
    return gamers.Skip(size*number).Take(size).ToList();
}

List<Gamer> Sort(SortEnum sort)
{
    return sort switch
    {
        SortEnum.NameAscending => gamers.OrderBy(x => x.Name).ToList(),
        SortEnum.NameDescending => gamers.OrderByDescending(x => x.Name).ToList(),
        SortEnum.LevelAscending => gamers.OrderBy(x => x.Level).ToList(),
        SortEnum.LevelDescending => gamers.OrderByDescending(x => x.Level).ToList(),
        SortEnum.ScoreAscending => gamers.OrderBy(x => x.Score).ToList(),
        SortEnum.ScoreDescending => gamers.OrderByDescending(x => x.Score).ToList(),
        SortEnum.HoursAscending => gamers.OrderBy(x => x.Hours).ToList(),
        SortEnum.HoursDescending => gamers.OrderByDescending(x => x.Hours).ToList(),
        _ => throw new NotImplementedException(),
    };
}

List<Gamer> Filter(string? name, int? level, int? score, int? hours)
{
    List<Gamer> buffer = gamers;
    if (name != null)
    {
        buffer = buffer.FindAll(x => x.Name == name);
    }
    if (level != null)
    {
        buffer = buffer.FindAll(x => x.Level == level);
    }
    if (score != null)
    {
        buffer = buffer.FindAll(x => x.Score == score);
    }
    if (hours != null)
    {
        buffer = buffer.FindAll(x => x.Hours == hours);
    }
    return buffer;
}

app.MapGet("gamer/param", (string? findValue, int? size, int? number, string? name, int? level, int? score, int? hours, SortEnum ? sort) =>
{
    if (sort != null)
        return Sort(sort.Value);
    if(name != null || level != null || score != null || hours != null)
        return Filter(name, level, score, hours);
    if (size != null && number != null)
        return Pagination(size.Value, number.Value);
    if (findValue != null)
        return Find(findValue);
    return gamers;
});

app.MapGet("filters", () =>
{
    List<string> name = gamers.Select(x => x.Name).Distinct().ToList();
    List<int> level = gamers.Select(x => x.Level).Distinct().ToList();
    List<int> score = gamers.Select(x => x.Score).Distinct().ToList();
    List<int> hours = gamers.Select(x => x.Hours).Distinct().ToList();
    return new
    {
        name,
        level,
        score,
        hours
    };
});

app.MapGet(gamerRest, async (int? id) =>
{
    using ApplicationContext db = new();
    if (id != null)
    {
        Gamer? gamer = await db.Gamers.FirstOrDefaultAsync(x => x.Id == id);
        if (gamer == null)
        {
            return Results.NotFound(new { message = "Игрок не найден" });
        }
        else
        {
            return Results.Json(gamer);
        }
    }
    else
    {
        return Results.Json(await db.Gamers.ToListAsync());
    }
});
app.MapPost(gamerRest, (Gamer newgamer) =>
{
    using ApplicationContext db = new();
    if (Regex.IsMatch(newgamer.Phone, pattern))
    {
        bool HavePhone = false;
        foreach (var gamer in gamers)
            if(newgamer.Phone == gamer.Phone)
            {
                HavePhone = true;
                break;
            }
        if(HavePhone == true)
        {
            return "Нельзя указать этот телефон";
        }
        else
        {
            db.Gamers.Add(newgamer);
            db.SaveChanges();
            return "Игрок успешно добавлен";
        }
    }
    else
    {
        return "Указан неверный номер телефона";
    }
});
app.MapPut(gamerRest, async (GamerUpdateDTO dto, int id) =>
{
    using ApplicationContext db = new();
    var change = await db.Gamers.FirstOrDefaultAsync(x => x.Id == id);
    if (change == null)
    {
        return Results.NotFound(new { message = "Игрок не найден" });
    }
    else
    {
        if (dto.Name != change.Name && dto.Name != "")
        {
            change.Name = dto.Name;
        }
        if (dto.Level != change.Level && dto.Level != null)
        {
            change.Level = dto.Level;
        }
        if (dto.Score != change.Score && dto.Score != null)
        {
            change.Score = dto.Score;
        }
        if (dto.Hours != change.Hours && dto.Hours != null)
        {
            change.Hours = dto.Hours;
        }
        if (dto.Phone != change.Phone && dto.Phone != "" && Regex.IsMatch(dto.Phone, pattern))
        {
            change.Phone = dto.Phone;
        }
        await db.SaveChangesAsync();
        return Results.Json(change);
    }
});
app.MapDelete(gamerRest, async (int id) =>
{
    using ApplicationContext db = new();
    Gamer? gamer = await db.Gamers.FirstOrDefaultAsync(x => x.Id == id);
    if (gamer == null)
    {
        return Results.NotFound(new { message = "Игрок не найден" });
    }
    else
    {
        db.Gamers.Remove(gamer);
        await db.SaveChangesAsync();
        return Results.Json(gamer);
    }
});

app.Run();

enum SortEnum
{
    NameAscending,
    NameDescending,
    LevelAscending,
    LevelDescending,
    ScoreAscending,
    ScoreDescending,
    HoursAscending,
    HoursDescending,
}

record class GamerUpdateDTO(string Name, int Level, int Score, int Hours, string Phone);
public class Gamer(int id, string name, int level, int score, int hours, string phone)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public int Level { get; set; } = level;
    public int Score { get; set; } = score;
    public int Hours { get; set; } = hours;
    public string Phone { get; set; } = phone;
}

public class ApplicationContext : DbContext
{
    public DbSet<Gamer> Gamers => Set<Gamer>();
    public ApplicationContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Gamer.db");
    }
}