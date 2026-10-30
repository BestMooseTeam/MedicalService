using MedicalSystem.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedicalSystem.Notifications
{
    internal class SmsNotifier : INotifier
    {
        public void Send(Patient patient, string message)
        {
            Console.WriteLine($"SMS для {patient.Name} ({patient.PhoneNumber}): {message}");
        }
    }
}
