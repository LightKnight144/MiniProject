using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

List<Technic> technic =
[
    new("Стиральная машина", "Егор", "Бытовая техника", new(2023, 8, 2), 10000),
    new("Плита", "Егор", "Бытовая техника", new(2024, 1, 1), 15000),
    new("Ноутбук", "Егор", "Вычислительная техника", new(2024, 1, 1), 25000),
];

string technicRepo = "technic";

app.MapGet(technicRepo, (int? index) =>
{
    if (index != null)
        return [technic[index.Value]];
    return technic;
});
app.MapPost(technicRepo, ([FromBody] Technic newtechnic) =>
{
    technic.Add(newtechnic);
});
app.MapPut(technicRepo, ([FromBody] TechnicUpdateDTO dto, int? index) =>
{
    var change = technic[index.Value];
    if (dto.Name != change.Name && dto.Name != "")
    {
        change.Name = dto.Name;
    }
    if (dto.Creator != change.Creator && dto.Creator != "")
    {
        change.Creator = dto.Creator;
    }
    if (dto.Category != change.Category && dto.Category != "")
    {
        change.Category = dto.Category;
    }
    if (dto.YearRelease != change.YearRelease)
    {
        change.YearRelease = dto.YearRelease;
    }
    if (dto.Price != change.Price)
    {
        change.Price = dto.Price;
    }
});
app.MapDelete(technicRepo, (int? index) =>
{
    if (index != null)
        technic.Remove(technic[index.Value]);
});
app.Run();
record class TechnicUpdateDTO(string Name, string Creator, string Category, DateOnly YearRelease, int Price);
class Technic(string name, string creator, string category, DateOnly yearRelease, int price)
{
    public string Name { get; set; } = name;
    public string Creator { get; set; } = creator;
    public string Category { get; set; } = category;
    public DateOnly YearRelease { get; set; } = yearRelease;
    public int Price { get; set; } = price;
}