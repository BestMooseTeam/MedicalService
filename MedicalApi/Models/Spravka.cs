namespace MedicalApi.Models;

public class Spravka
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int SpravkaTypeId { get; set; }

    // Заполняются сервером из каталога видов справок, присланные значения игнорируются
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ValidUntil { get; set; }
}
