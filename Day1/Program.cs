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