using Microsoft.AspNetCore.Mvc;
using MedicalApi.Models;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    // Тестовые записи на ближайшие дни
    public static List<Appointment> Appointments = new List<Appointment>
    {
        new Appointment { Id = 1, PatientId = 1, DoctorName = "Ахметова Гульнара", Specialty = "Терапевт",
            Time = WorkingDay(1, 10), Room = 105, Complaint = "Кашель, температура" },
        new Appointment { Id = 2, PatientId = 2, DoctorName = "Ким Елена", Specialty = "Педиатр",
            Time = WorkingDay(2, 9), Room = 210 },
    };

    private static int _nextId = Appointments.Max(a => a.Id) + 1;

    // GET /api/appointments?patientId=1&date=2026-11-10
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public List<Appointment> GetAll(int? patientId, DateTime? date)
    {
        IEnumerable<Appointment> result = Appointments;

        if (patientId != null)
        {
            result = result.Where(a => a.PatientId == patientId);
        }
        if (date != null)
        {
            result = result.Where(a => a.Time.Date == date.Value.Date);
        }

        return result.OrderBy(a => a.Time).ToList();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Appointment> GetById(int id)
    {
        Appointment? appointment = Appointments.FirstOrDefault(a => a.Id == id);
        if (appointment == null)
        {
            return NotFound();
        }
        return appointment;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    public ActionResult<Appointment> Create(Appointment appointment)
    {
        string? error = Validate(appointment, null);
        if (error != null)
        {
            return BadRequest(error);
        }

        appointment.Id = _nextId++;
        Appointments.Add(appointment);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Appointment> Update(int id, Appointment appointment)
    {
        Appointment? existing = Appointments.FirstOrDefault(a => a.Id == id);
        if (existing == null)
        {
            return NotFound();
        }

        string? error = Validate(appointment, existing);
        if (error != null)
        {
            return BadRequest(error);
        }

        existing.PatientId = appointment.PatientId;
        existing.DoctorName = appointment.DoctorName;
        existing.Specialty = appointment.Specialty;
        existing.Time = appointment.Time;
        existing.Room = appointment.Room;
        existing.Complaint = appointment.Complaint;
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Delete(int id)
    {
        Appointment? appointment = Appointments.FirstOrDefault(a => a.Id == id);
        if (appointment == null)
        {
            return NotFound();
        }

        Appointments.Remove(appointment);
        return NoContent();
    }

    // Рабочий день через days дней в hours:00, воскресенье пропускаем
    private static DateTime WorkingDay(int days, int hours)
    {
        DateTime day = DateTime.Today.AddDays(days);
        if (day.DayOfWeek == DayOfWeek.Sunday)
        {
            day = day.AddDays(1);
        }
        return day.AddHours(hours);
    }

    // existing — редактируемая запись, при создании null
    private string? Validate(Appointment appointment, Appointment? existing)
    {
        if (!PatientsController.Patients.Any(p => p.Id == appointment.PatientId))
        {
            return $"Пациент с Id {appointment.PatientId} не найден. Выберите пациента из картотеки";
        }

        if (string.IsNullOrWhiteSpace(appointment.DoctorName))
        {
            return "Укажите ФИО врача";
        }
        if (appointment.DoctorName.Length > 100)
        {
            return "ФИО врача не должно быть длиннее 100 символов";
        }
        if (string.IsNullOrWhiteSpace(appointment.Specialty))
        {
            return "Укажите специальность врача, например Терапевт";
        }
        if (appointment.Specialty.Length > 50)
        {
            return "Специальность не должна быть длиннее 50 символов";
        }

        // Время сравниваем как местное, даже если клиент прислал его с Z (UTC)
        DateTime time = appointment.Time.Kind == DateTimeKind.Utc ? appointment.Time.ToLocalTime() : appointment.Time;
        appointment.Time = DateTime.SpecifyKind(time, DateTimeKind.Unspecified);

        if (appointment.Time == default)
        {
            return "Укажите время приёма в формате 2026-11-10T09:30";
        }
        // Прошедшее время запрещаем, только если его меняют: старую запись можно поправить, не трогая время
        bool timeChanged = existing == null || existing.Time != appointment.Time;
        if (timeChanged && appointment.Time <= DateTime.Now)
        {
            return "Нельзя записать на прошедшее время";
        }
        if (appointment.Time.DayOfWeek == DayOfWeek.Sunday)
        {
            return "В воскресенье поликлиника не работает";
        }
        if (appointment.Time.Hour < 8 || appointment.Time.Hour >= 20)
        {
            return "Приём идёт с 08:00 до 20:00, последняя запись в 19:45";
        }
        if (appointment.Time.TimeOfDay.Ticks % TimeSpan.FromMinutes(15).Ticks != 0)
        {
            return "Запись идёт каждые 15 минут: выберите время вида 09:00, 09:15, 09:30 или 09:45";
        }

        if (appointment.Room < 1 || appointment.Room > 999)
        {
            return "Номер кабинета должен быть от 1 до 999";
        }

        int id = existing?.Id ?? 0;
        Appointment? doctorBusy = Appointments.FirstOrDefault(a => a.Id != id && a.Time == appointment.Time &&
            string.Equals(a.DoctorName.Trim(), appointment.DoctorName.Trim(), StringComparison.OrdinalIgnoreCase));
        if (doctorBusy != null)
        {
            return $"У врача {doctorBusy.DoctorName} на {appointment.Time:dd.MM.yyyy HH:mm} уже есть запись. Выберите другое время";
        }
        if (Appointments.Any(a => a.Id != id && a.Time == appointment.Time && a.PatientId == appointment.PatientId))
        {
            return $"Пациент уже записан на {appointment.Time:dd.MM.yyyy HH:mm} к другому врачу";
        }

        return null;
    }
}
