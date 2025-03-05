using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

List<Chocolate> chocolate =
[
    new("Шоколад", "Клубника", "Шоколадная фабрика", 100),
    new("Пирог", "Тыква", "Шоколадная фабрика", 150),
    new("Конфеты", "Черника", "Шоколадная фабрика", 200),
];

string chocolateRepo = "chocolate";

app.MapGet(chocolateRepo, (int? index) =>
{
    if (index != null)
        return [chocolate[index.Value]];
    return chocolate;
});
app.MapPost(chocolateRepo, ([FromBody] Chocolate newchocolate) =>
{
    chocolate.Add(newchocolate);
});
app.MapPut(chocolateRepo, ([FromBody] ChocolateUpdateDTO dto, int? index) =>
{
    var change = chocolate[index.Value];
    if (dto.Product != change.Product)
    {
        change.Product = dto.Product;
    }
    if (dto.Ingredient != change.Ingredient)
    {
        change.Ingredient = dto.Ingredient;
    }
    if (dto.Company != change.Company)
    {
        change.Company = dto.Company;
    }
    if (dto.Price != change.Price)
    {
        change.Price = dto.Price;
    }
});
app.MapDelete(chocolateRepo, (int? index) =>
{
    if (index != null)
        chocolate.Remove(chocolate[index.Value]);
});
app.Run();
class Chocolate(string product, string ingredient, string company, int price)
{
    public string Product { get; set; } = product;
    public string Ingredient { get; set; } = ingredient;
    public string Company { get; set; } = company;
    public int Price { get; set; } = price;
}
record class ChocolateUpdateDTO(string Product, string Ingredient, string Company, int Price);