using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

List<Order> orders =
[
    new("Кофемолка", "не работает", "описание", "Иван", "в ожидании", new(2024, 8, 2)),
    new("Холодильник", "не работает", "описание", "Иван", "в ожидании", new(2024, 3, 7)),
    new("Плита", "не работает", "описание", "Иван", "в ожидании", new(2024, 8, 5)),
];

string orderRepo = "order";

app.MapGet(orderRepo, (int? index) =>
{
    if (index != null)
      return [orders[index.Value]];
    return orders;
});
app.MapPost(orderRepo, ([FromBody] Order neworder) =>
{
    orders.Add(neworder);
});
app.MapPut(orderRepo, ([FromBody] OrderUpdateDTO dto, int? index) =>
{
var change = orders[index.Value];
if (dto.Status != change.Status)
{
    change.Status = dto.Status;
    if (dto.Status == "выполнено")
    {
        change.EndDate = DateOnly.FromDateTime(DateTime.Now);
    }
}
if (dto.Description != change.Description)
{
    change.Description = dto.Description;
}
if (dto.Master != change.Master)
{
    change.Master = dto.Master;
}
});
app.MapDelete(orderRepo, (int? index) =>
{
if (index != null)
        orders.Remove(orders[index.Value]);
});
app.Run();

record class OrderUpdateDTO(string Status, string Description, string Master);
class Order(string appliances, string problemType, string description, string client, string status, DateOnly startDate)
{
    public string Appliances { get; set; } = appliances;
    public string ProblemType { get; set; } = problemType;
    public string Description { get; set; } = description;
    public string Client { get; set; } = client;
    public string Status { get; set; } = status;
    public string Master { get; set; } = "Не назначено";
    public DateOnly StartDate { get; set; } = startDate;
    public DateOnly? EndDate { get; set; } = null;

}