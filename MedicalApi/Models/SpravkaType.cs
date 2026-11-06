namespace MedicalApi.Models;

public class SpravkaType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int ValidDays { get; set; }                 // сколько дней действует
    public bool NeedsDoctorExam { get; set; }          // нужен осмотр врача
    public int MinAge { get; set; } = 0;               // для какого возраста выдаётся
    public int MaxAge { get; set; } = 120;
}
