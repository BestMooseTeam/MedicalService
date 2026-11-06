namespace MedicalApi.Models;

// Запись на приём к врачу
public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string DoctorName { get; set; } = "";
    public string Specialty { get; set; } = "";
    public DateTime Time { get; set; }
    public int Room { get; set; }                      // номер кабинета
    public string? Complaint { get; set; }             // жалобы, необязательно
}
