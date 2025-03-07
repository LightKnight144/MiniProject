using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

List<Magazines> magazines =
[
    new("Кроссворд", "Головоломка", 15, "описание",  new(2024, 11, 2)),
    new("Здоровье", "Здоровье", 30, "описание",  new(2024, 2, 7)),
];

string magazineRepo = "magazine";

app.MapGet(magazineRepo, (int? index) =>
{
    if (index != null)
        return [magazines[index.Value]];
    return magazines;
});
app.MapPost(magazineRepo, ([FromBody] Magazines newmagazine) =>
{
    magazines.Add(newmagazine);
});
app.MapPut(magazineRepo, ([FromBody] MagazineUpdateDTO dto, int? index) =>
{
    var change = magazines[index.Value];
    if (dto.Name != change.Name && dto.Name != "")
    {
        change.Name = dto.Name;
    }
    if (dto.Type != change.Type && dto.Type != "")
    {
        change.Type = dto.Type;
    }
    if (dto.Number != change.Number)
    {
        change.Number = dto.Number;
    }
    if (dto.Description != change.Description && dto.Description != "")
    {
        change.Description = dto.Description;
    }
    if (dto.YearRelease != change.YearRelease)
    {
        change.YearRelease = dto.YearRelease;
    }
});
app.MapDelete(magazineRepo, (int? index) =>
{
    if (index != null)
        magazines.Remove(magazines[index.Value]);
});
app.Run();
class Magazines(string name, string type, int number, string description, DateOnly yearRelease)
{
    public string Name { get; set; } = name;
    public string Type { get; set; } = type;
    public int Number { get; set; } = number;
    public string Description { get; set; } = description;
    public DateOnly YearRelease { get; set; } = yearRelease;
}
record class MagazineUpdateDTO(string Name, string Type, int Number, string Description, DateOnly YearRelease);