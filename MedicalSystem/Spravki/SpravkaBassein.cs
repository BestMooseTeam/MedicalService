using MedicalSystem.Interfaces;
using MedicalSystem.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedicalSystem.Spravki
{
    internal class SpravkaBassein : ISpravka
    {
        public string SpravkaName { get; set; }
        public Patient Patient { get; set; }
        public decimal Price { get; set; }
        public int ValidMonths { get; set; }

        public SpravkaBassein(Patient patient, decimal price, int validMonths)
        {
            SpravkaName = "Spravka Bassein";
            Patient = patient;
            Price = price;
            ValidMonths = validMonths;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"Spravka Name: {SpravkaName}");
            Console.WriteLine($"Patient: {Patient.Name} {Patient.Surname}");
            Console.WriteLine($"Blood Type: {Patient.BloodType}");
            Console.WriteLine($"Price: {Price}");
            Console.WriteLine($"Valid Months: {ValidMonths}");
        }

        public decimal CalculatePrice()
        {
            return Price;
        }

        public string GetShortInfo()
        {
            return $"{SpravkaName}: {CalculatePrice():F2}";
        }
    }
}
