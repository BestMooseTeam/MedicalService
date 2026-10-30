using MedicalSystem.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedicalSystem.Interfaces
{
    internal interface ISpravka
    {
        string SpravkaName { get; set; }
        Patient Patient { get; set; }
        decimal Price { get; set; }

        void DisplayInfo();

        decimal CalculatePrice();

        string GetShortInfo();

    }
}
