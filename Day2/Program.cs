using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

string restaurantRest = "restaurant";
string pattern = @"^[^@\s]+\@test$";

app.MapGet(restaurantRest, async (int? id) =>
{
    using ApplicationContext db = new();
    if (id != null)
    {
        Restaurant? restaurant = await db.Restaurants.FirstOrDefaultAsync(x => x.Id == id);
        if (restaurant == null)
        {
            return Results.NotFound(new { message = "Ресторан не найден" });
        }
        else
        {
            return Results.Json(restaurant);
        }
    }
    else
    {
        return Results.Json(await db.Restaurants.ToListAsync());
    }
});
app.MapPost(restaurantRest, (Restaurant r) =>
{
    using ApplicationContext db = new();
    if (Regex.IsMatch(r.Mail, pattern))
    {
        if (r.Rank > 5 || r.Rank < 1)
        {
            return "Неправильно составлен рейтинг";
        }
        else
        {
            db.Restaurants.Add(r);
            db.SaveChanges();
            return "Ресторан успешно добавлен";
        }
    }
    else
    {
        return "Указана неверно почта";
    }
});
app.MapPut(restaurantRest, async (RestaurantUpdateDTO dto, int id) =>
{
    using ApplicationContext db = new();
    var change = await db.Restaurants.FirstOrDefaultAsync(x => x.Id == id);
    if (change == null)
    {
        return Results.NotFound(new { message = "Ресторан не найден" });
    }
    else
    {
        if (dto.Name != change.Name && dto.Name != "")
        {
            change.Name = dto.Name;
        }
        if (dto.Rank != change.Rank && dto.Rank != null)
        {
            change.Rank = dto.Rank;
            if (change.Rank > 5)
            {
                change.Rank = 5;
            }
            if (change.Rank < 1)
            {
                change.Rank = 1;
            }
        }
        if (dto.Street != change.Street && dto.Street != "")
        {
            change.Street = dto.Street;
        }
        if (dto.Creation != change.Creation)
        {
            change.Creation = dto.Creation;
        }

        if (dto.Mail != change.Mail && dto.Mail != "" && Regex.IsMatch(dto.Mail, pattern))
        {
            change.Mail = dto.Mail;
        }
        await db.SaveChangesAsync();
        return Results.Json(change);
    }
});
app.MapDelete(restaurantRest, async (int id) =>
{
    using ApplicationContext db = new();
    Restaurant? restaurant = await db.Restaurants.FirstOrDefaultAsync(x => x.Id == id);
    if (restaurant == null)
    {
        return Results.NotFound(new { message = "Ресторан не найден" });
    }
    else
    {
        db.Restaurants.Remove(restaurant);
        await db.SaveChangesAsync();
        return Results.Json(restaurant);
    }
});

app.Run();