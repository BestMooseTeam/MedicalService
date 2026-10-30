using MedicalSystem.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedicalSystem.Notifications
{
    internal class TelegramNotifier : INotifier
    {
        public void Send(Patient patient, string message)
        {
            Console.WriteLine($"Telegram для {patient.Name}: {message}");
        }
    }
}
