using MedicalSystem.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedicalSystem.Notifications
{
    internal interface INotifier
    {
        void Send(Patient patient, string message);
    }
}
