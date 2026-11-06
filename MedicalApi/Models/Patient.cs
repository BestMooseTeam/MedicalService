namespace MedicalApi.Models;

public class Patient
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public string Iin { get; set; } = "";              // ИИН, 12 цифр
    public string Phone { get; set; } = "";
    public string? Address { get; set; }               // необязательное поле
    public int DistrictNumber { get; set; }            // номер участка
    public bool HasInsurance { get; set; }             // застрахован в ОСМС

    // Метод, а не свойство: в JSON и в Swagger не попадает
    public int GetAge()
    {
        DateTime today = DateTime.Today;
        int age = today.Year - BirthDate.Year;
        if (BirthDate.Date > today.AddYears(-age))
        {
            age--;
        }
        return age;
    }
}
