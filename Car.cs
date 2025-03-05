using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

List<Car1> cars =
[
    new("Волга", "Серый", 2000, 10000),
    new("Lada", "Синий", 2009, 15000),
    new("Москвич", "Красный", 2005, 20000),
];

string carRepo = "cars";

app.MapGet(carRepo, (int? index) =>
{
    if (index != null)
        return [cars[index.Value]];
    return cars;
});
app.MapPost(carRepo, ([FromBody] Car1 newcar) => 
{
    cars.Add(newcar);
});
app.MapPut(carRepo, ([FromBody] CarUpdateDTO dto, int? index) =>
{
    var change = cars[index.Value];
    if (dto.Mark != change.Mark)
    {
        change.Mark = dto.Mark;
    }
    if (dto.Color != change.Color)
    {
        change.Color = dto.Color;
    }
    if (dto.Year != change.Year)
    {
        change.Year = dto.Year;
    }
    if (dto.Price != change.Price)
    {
        change.Price = dto.Price;
    }
});
app.MapDelete(carRepo, (int? index) =>
{
    if (index != null)
        cars.Remove(cars[index.Value]);
});
app.Run();
class Car1(string mark, string color, int year, int price)
{
    public string Mark { get; set; } = mark;
    public string Color { get; set; } = color;
    public int Year { get; set; } = year;
    public int Price { get; set; } = price;

}
record class CarUpdateDTO(string Mark, string Color, int Year, int Price);

