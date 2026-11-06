using Microsoft.AspNetCore.Mvc;
using MedicalApi.Models;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpravkiController : ControllerBase
{
    // Тестовые данные: справки пациентов из PatientsController
    public static List<Spravka> Spravki = new List<Spravka>
    {
        FillFromCatalog(new Spravka { Id = 1, PatientId = 1, SpravkaTypeId = 1, IssueDate = DateTime.Today.AddDays(-30) }),
        FillFromCatalog(new Spravka { Id = 2, PatientId = 1, SpravkaTypeId = 3, IssueDate = DateTime.Today.AddDays(-5) }),
        FillFromCatalog(new Spravka { Id = 3, PatientId = 3, SpravkaTypeId = 2, IssueDate = DateTime.Today.AddDays(-100) }),
    };

    // Не Count + 1: после удаления справки новая получала бы номер существующей
    private static int _nextId = Spravki.Max(s => s.Id) + 1;

    // GET /api/spravki?patientId=1&sortBy=price&descending=true
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    public ActionResult<List<Spravka>> GetAll(int? patientId, string? sortBy, bool descending = false)
    {
        IEnumerable<Spravka> result = Spravki;

        if (patientId != null)
        {
            result = result.Where(s => s.PatientId == patientId);
        }

        if (sortBy == "price")
        {
            result = descending ? result.OrderByDescending(s => s.Price) : result.OrderBy(s => s.Price);
        }
        else if (sortBy == "date")
        {
            result = descending ? result.OrderByDescending(s => s.IssueDate) : result.OrderBy(s => s.IssueDate);
        }
        else if (sortBy != null)
        {
            return BadRequest("Сортировать можно только по price (цена) или date (дата выдачи)");
        }

        return result.ToList();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Spravka> GetById(int id)
    {
        Spravka? spravka = Spravki.FirstOrDefault(s => s.Id == id);
        if (spravka == null)
        {
            return NotFound();
        }
        return spravka;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    public ActionResult<Spravka> Create(Spravka spravka)
    {
        string? error = Validate(spravka);
        if (error != null)
        {
            return BadRequest(error);
        }

        spravka.Id = _nextId++;
        spravka.IssueDate = DateTime.Today;
        FillFromCatalog(spravka);
        Spravki.Add(spravka);
        return CreatedAtAction(nameof(GetById), new { id = spravka.Id }, spravka);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Spravka> Update(int id, Spravka spravka)
    {
        Spravka? existing = Spravki.FirstOrDefault(s => s.Id == id);
        if (existing == null)
        {
            return NotFound();
        }

        string? error = Validate(spravka);
        if (error != null)
        {
            return BadRequest(error);
        }

        // Дата выдачи остаётся прежней, название, цена и срок действия — из каталога
        existing.PatientId = spravka.PatientId;
        existing.SpravkaTypeId = spravka.SpravkaTypeId;
        FillFromCatalog(existing);
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Delete(int id)
    {
        Spravka? spravka = Spravki.FirstOrDefault(s => s.Id == id);
        if (spravka == null)
        {
            return NotFound();
        }

        Spravki.Remove(spravka);
        return NoContent();
    }

    // Название, цену и срок действия берём из каталога, а не из тела запроса.
    // Вызывать только после Validate: вид справки должен существовать
    private static Spravka FillFromCatalog(Spravka spravka)
    {
        SpravkaType type = SpravkaTypesController.Types.First(t => t.Id == spravka.SpravkaTypeId);
        spravka.Name = type.Name;
        spravka.Price = type.Price;
        spravka.ValidUntil = spravka.IssueDate.AddDays(type.ValidDays);
        return spravka;
    }

    private string? Validate(Spravka spravka)
    {
        Patient? patient = PatientsController.Patients.FirstOrDefault(p => p.Id == spravka.PatientId);
        if (patient == null)
        {
            return $"Пациент с Id {spravka.PatientId} не найден. Выберите пациента из картотеки";
        }

        SpravkaType? type = SpravkaTypesController.Types.FirstOrDefault(t => t.Id == spravka.SpravkaTypeId);
        if (type == null)
        {
            return $"Вид справки с Id {spravka.SpravkaTypeId} не найден. Выберите вид из каталога /api/SpravkaTypes";
        }

        int age = patient.GetAge();
        if (age < type.MinAge || age > type.MaxAge)
        {
            return $"Справка «{type.Name}» выдаётся в возрасте от {type.MinAge} до {type.MaxAge} лет, " +
                $"а пациенту {patient.Name} сейчас {age}";
        }

        return null;
    }
}
