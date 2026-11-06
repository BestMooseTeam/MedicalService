using Microsoft.AspNetCore.Mvc;
using MedicalApi.Models;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    // Тестовые данные, чтобы после запуска картотека не была пустой
    public static List<Patient> Patients = new List<Patient>
    {
        new Patient { Id = 1, Name = "Асанов Актан", BirthDate = new DateTime(1990, 3, 12), Iin = "900312350127",
            Phone = "+7 701 123 45 67", Address = "ул. Абая, 10", DistrictNumber = 5, HasInsurance = true },
        new Patient { Id = 2, Name = "Иванова Мария", BirthDate = new DateTime(2015, 7, 1), Iin = "150701650214",
            Phone = "+7 705 765 43 21", DistrictNumber = 3, HasInsurance = true },
        new Patient { Id = 3, Name = "Сериков Даурен", BirthDate = new DateTime(1985, 11, 23), Iin = "851123300456",
            Phone = "+7 777 000 11 22", Address = "мкр. Самал-2, 15", DistrictNumber = 12, HasInsurance = false },
    };

    // Следующий свободный номер. Не Count + 1: после удаления номера бы повторялись
    private static int _nextId = Patients.Max(p => p.Id) + 1;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public List<Patient> GetAll()
    {
        return Patients;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Patient> GetById(int id)
    {
        Patient? patient = Patients.FirstOrDefault(p => p.Id == id);
        if (patient == null)
        {
            return NotFound();
        }
        return patient;
    }

    [HttpGet("{id}/spravki")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<Spravka>> GetSpravki(int id)
    {
        Patient? patient = Patients.FirstOrDefault(p => p.Id == id);
        if (patient == null)
        {
            return NotFound();
        }
        return SpravkiController.Spravki.Where(s => s.PatientId == id).ToList();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    public ActionResult<Patient> Create(Patient patient)
    {
        string? error = Validate(patient, 0);
        if (error != null)
        {
            return BadRequest(error);
        }

        patient.Id = _nextId++;
        Patients.Add(patient);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Patient> Update(int id, Patient patient)
    {
        Patient? existing = Patients.FirstOrDefault(p => p.Id == id);
        if (existing == null)
        {
            return NotFound();
        }

        string? error = Validate(patient, id);
        if (error != null)
        {
            return BadRequest(error);
        }

        existing.Name = patient.Name;
        existing.BirthDate = patient.BirthDate;
        existing.Iin = patient.Iin;
        existing.Phone = patient.Phone;
        existing.Address = patient.Address;
        existing.DistrictNumber = patient.DistrictNumber;
        existing.HasInsurance = patient.HasInsurance;
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status409Conflict, "text/plain")]
    public ActionResult Delete(int id)
    {
        Patient? patient = Patients.FirstOrDefault(p => p.Id == id);
        if (patient == null)
        {
            return NotFound();
        }

        // Справки и записи на приём — медицинская история, молча её не удаляем
        int spravkiCount = SpravkiController.Spravki.Count(s => s.PatientId == id);
        int appointmentsCount = AppointmentsController.Appointments.Count(a => a.PatientId == id);
        if (spravkiCount > 0 || appointmentsCount > 0)
        {
            return Conflict($"Нельзя удалить пациента: у него справок {spravkiCount}, записей на приём {appointmentsCount}. " +
                "Сначала удалите их");
        }

        Patients.Remove(patient);
        return NoContent();
    }

    // id — номер редактируемого пациента, чтобы ИИН не считался дублем самого себя. При создании 0
    private string? Validate(Patient patient, int id)
    {
        if (string.IsNullOrWhiteSpace(patient.Name))
        {
            return "Укажите ФИО пациента";
        }
        if (patient.Name.Length > 100)
        {
            return "ФИО не должно быть длиннее 100 символов";
        }

        if (patient.BirthDate == default)
        {
            return "Укажите дату рождения в формате 2000-05-15";
        }
        if (patient.BirthDate > DateTime.Now)
        {
            return "Дата рождения не может быть в будущем";
        }
        if (patient.BirthDate < DateTime.Today.AddYears(-120))
        {
            return "Проверьте дату рождения: пациенту получается больше 120 лет";
        }

        if (string.IsNullOrWhiteSpace(patient.Iin))
        {
            return "Укажите ИИН пациента";
        }
        if (patient.Iin.Length != 12 || !patient.Iin.All(char.IsAsciiDigit))
        {
            return "ИИН должен состоять ровно из 12 цифр, без пробелов";
        }
        string birthDigits = patient.BirthDate.ToString("yyMMdd");
        if (!patient.Iin.StartsWith(birthDigits))
        {
            return $"Первые 6 цифр ИИН — дата рождения в виде ГГММДД. Для этой даты ИИН должен начинаться с {birthDigits}";
        }
        Patient? sameIin = Patients.FirstOrDefault(p => p.Iin == patient.Iin && p.Id != id);
        if (sameIin != null)
        {
            return $"Пациент с ИИН {patient.Iin} уже есть в картотеке: {sameIin.Name}, Id {sameIin.Id}";
        }

        if (string.IsNullOrWhiteSpace(patient.Phone))
        {
            return "Укажите телефон";
        }
        if (!patient.Phone.All(c => char.IsAsciiDigit(c) || " +-()".Contains(c)))
        {
            return "Телефон может содержать только цифры, пробелы и символы + - ( )";
        }
        int phoneDigits = patient.Phone.Count(char.IsAsciiDigit);
        if (phoneDigits < 10 || phoneDigits > 15)
        {
            return "В номере телефона должно быть от 10 до 15 цифр, например +7 701 123 45 67";
        }

        if (patient.Address != null && patient.Address.Length > 200)
        {
            return "Адрес не должен быть длиннее 200 символов";
        }

        if (patient.DistrictNumber < 1 || patient.DistrictNumber > 100)
        {
            return "Номер участка должен быть от 1 до 100";
        }

        return null;
    }
}
