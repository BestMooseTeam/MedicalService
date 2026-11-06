using Microsoft.AspNetCore.Mvc;
using MedicalApi.Models;

namespace MedicalApi.Controllers;

// Статистика для главврача: сколько справок выписано и на какую сумму
[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Statistics Get()
    {
        List<Spravka> spravki = SpravkiController.Spravki;
        return new Statistics
        {
            PatientsCount = PatientsController.Patients.Count,
            SpravkiCount = spravki.Count,
            SpravkiTotal = spravki.Sum(s => s.Price),
            AppointmentsCount = AppointmentsController.Appointments.Count,
            ByType = spravki
                .GroupBy(s => s.Name)
                .Select(g => new SpravkaTypeStatistics { Name = g.Key, Count = g.Count(), Total = g.Sum(s => s.Price) })
                .OrderByDescending(t => t.Total)
                .ToList(),
        };
    }
}
