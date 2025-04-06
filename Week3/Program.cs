using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

string serviceRest = "service";
string pattern = @"^\+?\d{1,3}[-.\s]?\(?\d{1,4}?\)?\d{1,4}\d{1,9}$";

List<Service> services = new List<Service>();
{
    using ApplicationContext db = new();
    foreach (var service in db.Services)
        services.Add(service);
}
List<Service> Find(string value)
{
    List<Service> buffer = [];
    foreach(var service in services)
        if (service.Name.Contains(value) || service.Specialty.Contains(value) || service.Task.Contains(value)
            || service.Experience.ToString().Contains(value))
            buffer.Add(service);
    return buffer;
}

List<Service> Pagination(int size, int number)
{
    List<Service> buffer = [];
    int next = size * number;
    for (int i = next; i < next + size; i++)
    {
        if (i >= services.Count)
            break;
        buffer.Add(services[i]);
    }
    return buffer;
}

List<Service> Sort(SortEnum sort)
{
    return sort switch
    {
        SortEnum.NameAscending => services.OrderBy(x => x.Name).ToList(),
        SortEnum.NameDescending => services.OrderByDescending(x => x.Name).ToList(),
        SortEnum.SpecialtyAscending => services.OrderBy(x => x.Specialty).ToList(),
        SortEnum.SpecialtyDescending => services.OrderByDescending(x => x.Specialty).ToList(),
        SortEnum.TaskAscending => services.OrderBy(x => x.Task).ToList(),
        SortEnum.TaskDescending => services.OrderByDescending(x => x.Task).ToList(),
        SortEnum.ExperienceAscending => services.OrderBy(x => x.Experience).ToList(),
        SortEnum.ExperienceDescending => services.OrderByDescending(x => x.Experience).ToList(),
        _ => throw new NotImplementedException(),
    };
}

List<Service> Filter(string? name, string? specialty, string? task, int? experience)
{
    List<Service> buffer = services;
    if (name != null)
    {
        buffer = buffer.FindAll(x => x.Name == name);
    }
    if (specialty != null)
    {
        buffer = buffer.FindAll(x => x.Specialty == specialty);
    }
    if (task != null)
    {
        buffer = buffer.FindAll(x => x.Task == task);
    }
    if (experience != null)
    {
        buffer = buffer.FindAll(x => x.Experience == experience);
    }
    return buffer;
}

app.MapGet("service/param", (string? findValue, int? size, int? number, string? name, string? specialty, string? task, int ? experience, SortEnum ? sort) =>
{
    if (sort != null)
        return Sort(sort.Value);
    if(name != null || specialty != null || task != null || experience != null)
        return Filter(name, specialty, task, experience);
    if (size != null && number != null)
        return Pagination(size.Value, number.Value);
    if (findValue != null)
        return Find(findValue);
    return services;
});

app.MapGet("filters", () =>
{
    List<string> name = services.Select(x => x.Name).Distinct().ToList();
    List<string> specialty = services.Select(x => x.Specialty).Distinct().ToList();
    List<string> task = services.Select(x => x.Task).Distinct().ToList();
    List<int> experience = services.Select(x => x.Experience).Distinct().ToList();
    return new
    {
        name,
        specialty,
        task,
        experience
    };
});

app.MapGet(serviceRest, async (int? id) =>
{
    using ApplicationContext db = new();
    if (id != null)
    {
        Service? service = await db.Services.FirstOrDefaultAsync(x => x.Id == id);
        if (service == null)
        {
            return Results.NotFound(new { message = "Пользователь не найден" });
        }
        else
        {
            return Results.Json(service);
        }
    }
    else
    {
        return Results.Json(await db.Services.ToListAsync());
    }
});
app.MapPost(serviceRest, (Service newservice) =>
{
    using ApplicationContext db = new();
    if (Regex.IsMatch(newservice.Phone, pattern))
    {
        db.Services.Add(newservice);
        db.SaveChanges();
        return "Успешно добавлен сервис";
    }
    else
    {
        return "Указан неверный номер телефона";
    }
});
app.MapPut(serviceRest, async (ServiceUpdateDTO dto, int id) =>
{
    using ApplicationContext db = new();
    var change = await db.Services.FirstOrDefaultAsync(x => x.Id == id);
    if (change == null)
    {
        return Results.NotFound(new { message = "Пользователь не найден" });
    }
    else
    {
        if (dto.Name != change.Name && dto.Name != "")
        {
            change.Name = dto.Name;
        }
        if (dto.Specialty != change.Specialty && dto.Specialty != "")
        {
            change.Specialty = dto.Specialty;
        }
        if (dto.Task != change.Task && dto.Task != "")
        {
            change.Task = dto.Task;
        }
        if (dto.Experience != change.Experience)
        {
            change.Experience = dto.Experience;
        }
        if (dto.Phone != change.Phone && dto.Phone != "" && Regex.IsMatch(dto.Phone, pattern))
        {
            change.Phone = dto.Phone;
        }
        await db.SaveChangesAsync();
        return Results.Json(change);
    }
});
app.MapDelete(serviceRest, async (int id) =>
{
    using ApplicationContext db = new();
    Service? service = await db.Services.FirstOrDefaultAsync(x => x.Id == id);
    if (service == null)
    {
        return Results.NotFound(new { message = "Пользователь не найден" });
    }
    else
    {
        db.Services.Remove(service);
        await db.SaveChangesAsync();
        return Results.Json(service);
    }
});

app.Run();

enum SortEnum
{
    NameAscending,
    NameDescending,
    SpecialtyAscending,
    SpecialtyDescending,
    TaskAscending,
    TaskDescending,
    ExperienceAscending,
    ExperienceDescending,
}

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