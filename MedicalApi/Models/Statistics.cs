namespace MedicalApi.Models;

// Статистика для главврача
public class Statistics
{
    public int PatientsCount { get; set; }
    public int SpravkiCount { get; set; }
    public decimal SpravkiTotal { get; set; }
    public int AppointmentsCount { get; set; }
    public List<SpravkaTypeStatistics> ByType { get; set; } = new List<SpravkaTypeStatistics>();
}

public class SpravkaTypeStatistics
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
    public decimal Total { get; set; }
}
