using System;
using System.Collections.Generic;
using System.Text;
using MedicalSystem.Interfaces;

namespace MedicalSystem.Buhgalteria
{
    internal class Kassa
    {
        public decimal Total(List<ISpravka> spravkas)
        {
            decimal total = 0;
            foreach (var spravka in spravkas)
            {
                total += spravka.CalculatePrice();
            }
            return total;
        }

        public void PrintCheck(List<ISpravka> spravki)
        {
            foreach (var spravka in spravki)
            {
                Console.WriteLine(spravka.GetShortInfo());
            }
            Console.WriteLine($"Total: {Total(spravki):F2}");
        }
    }
}
