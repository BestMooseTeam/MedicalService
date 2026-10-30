using System;
using System.Collections.Generic;
using System.Text;
using MedicalSystem.Interfaces;
using MedicalSystem.Users;

namespace MedicalSystem.Spravki
{
    internal class SpravkaSanknijka: ISpravka
    {
        public string SpravkaName { get; set; }
        public Patient Patient { get; set; }
        public decimal Price { get; set; }
        public int AnalysisCount { get; set; } 

        public void DisplayInfo()
        {
            Console.WriteLine($"Spravka Name: {SpravkaName}");
            Console.WriteLine($"Patient: {Patient.Name} {Patient.Surname}");
            Console.WriteLine($"Price: {Price}");
        }

        public decimal CalculatePrice()
        {
            return Price + (AnalysisCount * 100);
        }

        public string GetShortInfo()
        {
            return $"{SpravkaName}: {CalculatePrice():F2}";
        }
    }
}
