using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

string hotelRest = "hotel";
string pattern = @"^\+?\d{1,3}[-.\s]?\(?\d{1,4}?\)?\d{1,4}\d{1,9}$";

List<Hotel> hotels = new List<Hotel>();
{
    using ApplicationContext db = new();
    foreach (var hotel in db.Hotels)
        hotels.Add(hotel);
}
List<Hotel> Find(string value)
{
    return hotels.FindAll(hotel =>
    hotel.Name.Contains(value) ||
    hotel.Rank.ToString().Contains(value) ||
    hotel.Street.Contains(value) ||
    hotel.Peoples.ToString().Contains(value));
}

List<Hotel> Pagination(int size, int number)
{
    return hotels.Skip(size*number).Take(size).ToList();
}

List<Hotel> Sort(SortEnum sort)
{
    return sort switch
    {
        SortEnum.NameAscending => hotels.OrderBy(x => x.Name).ToList(),
        SortEnum.NameDescending => hotels.OrderByDescending(x => x.Name).ToList(),
        SortEnum.RankAscending => hotels.OrderBy(x => x.Rank).ToList(),
        SortEnum.RankDescending => hotels.OrderByDescending(x => x.Rank).ToList(),
        SortEnum.StreetAscending => hotels.OrderBy(x => x.Street).ToList(),
        SortEnum.StreetDescending => hotels.OrderByDescending(x => x.Street).ToList(),
        SortEnum.PeoplesAscending => hotels.OrderBy(x => x.Peoples).ToList(),
        SortEnum.PeoplesDescending => hotels.OrderByDescending(x => x.Peoples).ToList(),
        _ => throw new NotImplementedException(),
    };
}

List<Hotel> Filter(string? name, int? rank, string? street, int? peoples)
{
    List<Hotel> buffer = hotels;
    if (name != null)
    {
        buffer = buffer.FindAll(x => x.Name == name);
    }
    if (rank != null)
    {
        buffer = buffer.FindAll(x => x.Rank == rank);
    }
    if (street != null)
    {
        buffer = buffer.FindAll(x => x.Street == street);
    }
    if (peoples != null)
    {
        buffer = buffer.FindAll(x => x.Peoples == peoples);
    }
    return buffer;
}

app.MapGet("hotel/param", (string? findValue, int? size, int? number, string? name, int? rank, string? street, int ? peoples, SortEnum ? sort) =>
{
    if (sort != null)
        return Sort(sort.Value);
    if(name != null || rank != null || street != null || peoples != null)
        return Filter(name, rank, street, peoples);
    if (size != null && number != null)
        return Pagination(size.Value, number.Value);
    if (findValue != null)
        return Find(findValue);
    return hotels;
});

app.MapGet("filters", () =>
{
    List<string> name = hotels.Select(x => x.Name).Distinct().ToList();
    List<int> rank = hotels.Select(x => x.Rank).Distinct().ToList();
    List<string> street = hotels.Select(x => x.Street).Distinct().ToList();
    List<int> peoples = hotels.Select(x => x.Peoples).Distinct().ToList();
    return new
    {
        name,
        rank,
        street,
        peoples
    };
});

app.MapGet(hotelRest, async (int? id) =>
{
    using ApplicationContext db = new();
    if (id != null)
    {
        Hotel? hotel = await db.Hotels.FirstOrDefaultAsync(x => x.Id == id);
        if (hotel == null)
        {
            return Results.NotFound(new { message = "Отель не найден" });
        }
        else
        {
            return Results.Json(hotel);
        }
    }
    else
    {
        return Results.Json(await db.Hotels.ToListAsync());
    }
});
app.MapPost(hotelRest, (Hotel newhotel) =>
{
    using ApplicationContext db = new();
    if (Regex.IsMatch(newhotel.Phone, pattern))
    {
        bool HavePhone = false;
        foreach (var hotel in hotels)
            if(newhotel.Phone == hotel.Phone)
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
            db.Hotels.Add(newhotel);
            db.SaveChanges();
            return "Отель успешно добавлен";
        }
    }
    else
    {
        return "Указан неверный номер телефона";
    }
});
app.MapPut(hotelRest, async (HotelUpdateDTO dto, int id) =>
{
    using ApplicationContext db = new();
    var change = await db.Hotels.FirstOrDefaultAsync(x => x.Id == id);
    if (change == null)
    {
        return Results.NotFound(new { message = "Отель не найден" });
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
        }
        if (dto.Street != change.Street && dto.Street != "")
        {
            change.Street = dto.Street;
        }
        if (dto.Peoples != change.Peoples && dto.Peoples != null)
        {
            change.Peoples = dto.Peoples;
        }
        if (dto.Phone != change.Phone && dto.Phone != "" && Regex.IsMatch(dto.Phone, pattern))
        {
            change.Phone = dto.Phone;
        }
        await db.SaveChangesAsync();
        return Results.Json(change);
    }
});
app.MapDelete(hotelRest, async (int id) =>
{
    using ApplicationContext db = new();
    Hotel? hotel = await db.Hotels.FirstOrDefaultAsync(x => x.Id == id);
    if (hotel == null)
    {
        return Results.NotFound(new { message = "Отель не найден" });
    }
    else
    {
        db.Hotels.Remove(hotel);
        await db.SaveChangesAsync();
        return Results.Json(hotel);
    }
});

app.Run();

enum SortEnum
{
    NameAscending,
    NameDescending,
    RankAscending,
    RankDescending,
    StreetAscending,
    StreetDescending,
    PeoplesAscending,
    PeoplesDescending,
}

record class HotelUpdateDTO(string Name, int Rank, string Street, int Peoples, string Phone);
public class Hotel(int id, string name, int rank, string street, int peoples, string phone)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public int Rank { get; set; } = rank;
    public string Street { get; set; } = street;
    public int Peoples { get; set; } = peoples;
    public string Phone { get; set; } = phone;
}

public class ApplicationContext : DbContext
{
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public ApplicationContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Hotel.db");
    }
}