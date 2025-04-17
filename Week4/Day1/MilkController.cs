using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.RegularExpressions;

namespace MilkProject;

[ApiController]
[Route("api/[controller]")]
public class MilkController : ControllerBase
{
    private MilkRepository repo = new();

    [HttpGet]
    public IActionResult Get(string? findValue, int? size, int? number, string? name, int? liter, int? price, SortEnum? sort)
    {
        var result = repo.Read(findValue, size, number, name, liter, price, sort);
        return Ok(result);
    }

    [HttpGet("find/{id}")]
    public IActionResult GetMilk(Guid id) 
    {
        Milk milk = repo.ReadById(id);
        return Ok(milk);
    }

    [HttpPut]
    public IActionResult Put(MilkUpdateDTO dto)
    {
        string check = @"^[^@\s]+\@test$";
        if (Regex.IsMatch(dto.Mail, check) && dto.Price > 0)
        {
            repo.Update(dto);
            return Ok();
        }
        else
        {
            return BadRequest("Incorrect values");
        }
    }

    [HttpDelete]
    public IActionResult DeleteMilk(Guid id)
    {
        repo.Delete(id);
        return Ok();
    }

    [HttpGet("filters")]
    public IActionResult GetFilters()
    {
        List<string> names = repo.ReadAll().Select(x => x.Name).Distinct().ToList();
        List<int> liters = repo.ReadAll().Select(x => x.Liter).Distinct().ToList();
        List<int> prices = repo.ReadAll().Select(x => x.Price).Distinct().ToList();
        return Ok(new
        {
            names,
            liters,
            prices
        });
    }

    [HttpPost]
    public IActionResult Post(MilkCreateDTO dto)
    {
        string check = @"^[^@\s]+\@test$";
        if (Regex.IsMatch(dto.Mail, check))
        {
            repo.Create(dto);
            return Ok();
        }
        else
        {
            return BadRequest("Mail is incorrect");
        }
    }
}
