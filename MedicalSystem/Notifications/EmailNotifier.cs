using MedicalSystem.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedicalSystem.Notifications
{
    internal class EmailNotifier : INotifier
    {
        public void Send(Patient patient, string message)
        {
            Console.WriteLine($"Email для {patient.Name}: {message}");
        }
    }
}
