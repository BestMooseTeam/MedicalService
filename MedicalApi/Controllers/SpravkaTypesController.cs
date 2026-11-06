using Microsoft.AspNetCore.Mvc;
using MedicalApi.Models;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpravkaTypesController : ControllerBase
{
    // Каталог заполнен в коде, чтобы не пропадал после перезапуска сервера
    public static List<SpravkaType> Types = new List<SpravkaType>
    {
        new SpravkaType { Id = 1, Name = "Справка 083", Price = 100, ValidDays = 365, NeedsDoctorExam = true, MinAge = 18 },
        new SpravkaType { Id = 2, Name = "Санитарная книжка", Price = 750, ValidDays = 365, NeedsDoctorExam = true, MinAge = 16 },
        new SpravkaType { Id = 3, Name = "Справка в бассейн", Price = 300, ValidDays = 90, NeedsDoctorExam = false },
        new SpravkaType { Id = 4, Name = "Справка 086 для поступления", Price = 1500, ValidDays = 180, NeedsDoctorExam = true, MinAge = 14 },
        new SpravkaType { Id = 5, Name = "Справка в детский лагерь", Price = 500, ValidDays = 3, NeedsDoctorExam = true, MinAge = 6, MaxAge = 17 },
    };

    private static int _nextId = Types.Max(t => t.Id) + 1;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public List<SpravkaType> GetAll()
    {
        return Types;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<SpravkaType> GetById(int id)
    {
        SpravkaType? type = Types.FirstOrDefault(t => t.Id == id);
        if (type == null)
        {
            return NotFound();
        }
        return type;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    public ActionResult<SpravkaType> Create(SpravkaType type)
    {
        string? error = Validate(type, 0);
        if (error != null)
        {
            return BadRequest(error);
        }

        type.Id = _nextId++;
        Types.Add(type);
        return CreatedAtAction(nameof(GetById), new { id = type.Id }, type);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<SpravkaType> Update(int id, SpravkaType type)
    {
        SpravkaType? existing = Types.FirstOrDefault(t => t.Id == id);
        if (existing == null)
        {
            return NotFound();
        }

        string? error = Validate(type, id);
        if (error != null)
        {
            return BadRequest(error);
        }

        existing.Name = type.Name;
        existing.Price = type.Price;
        existing.ValidDays = type.ValidDays;
        existing.NeedsDoctorExam = type.NeedsDoctorExam;
        existing.MinAge = type.MinAge;
        existing.MaxAge = type.MaxAge;
        return Ok(existing);
    }

    // Уже выписанные справки хранят своё название и цену, поэтому вид из каталога можно удалить
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Delete(int id)
    {
        SpravkaType? type = Types.FirstOrDefault(t => t.Id == id);
        if (type == null)
        {
            return NotFound();
        }

        Types.Remove(type);
        return NoContent();
    }

    private string? Validate(SpravkaType type, int id)
    {
        if (string.IsNullOrWhiteSpace(type.Name))
        {
            return "Укажите название вида справки";
        }
        if (type.Name.Length > 100)
        {
            return "Название не должно быть длиннее 100 символов";
        }
        bool nameTaken = Types.Any(t => t.Id != id &&
            string.Equals(t.Name.Trim(), type.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        if (nameTaken)
        {
            return $"Вид справки «{type.Name.Trim()}» уже есть в каталоге";
        }

        if (type.Price <= 0)
        {
            return "Цена должна быть больше 0";
        }
        if (type.Price > 1_000_000)
        {
            return "Цена не может быть больше 1 000 000 тенге. Проверьте, нет ли лишних нулей";
        }

        if (type.ValidDays < 1 || type.ValidDays > 3650)
        {
            return "Срок действия должен быть от 1 до 3650 дней";
        }

        if (type.MinAge < 0 || type.MinAge > 120)
        {
            return "Минимальный возраст должен быть от 0 до 120 лет";
        }
        if (type.MaxAge < type.MinAge || type.MaxAge > 120)
        {
            return $"Максимальный возраст должен быть от {type.MinAge} (минимального) до 120 лет";
        }

        return null;
    }
}
