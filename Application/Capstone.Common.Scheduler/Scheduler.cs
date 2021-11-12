using Capstone.Common.Scheduler.Models;
using CapstoneMedicareNotification.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace Capstone.Common.Scheduler
{
    public static class Scheduler
    {
        [FunctionName("MedicareNotification")]
        public static void Run([TimerTrigger("0 15 1 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"MedicareNotification function executed at: {DateTime.UtcNow}");
            using (ProdaDBContext tCtx = new ProdaDBContext())
            {
                var prodaColl = tCtx.ProdaTokenProperties;

                foreach (ProdaTokenProperty prodaTokenProperty in prodaColl)
                {
                    DateTime deviceExp = (DateTime)prodaTokenProperty.DeviceExpiry;
                    string deviceName = prodaTokenProperty.DeviceName;
                    CheckDeviceExpiry(deviceName, deviceExp.Date,log);   
                }
            }
        }

        private static void CheckDeviceExpiry(string deviceName, DateTime deviceExp, ILogger log)
        {
            TimeSpan timeSpan = deviceExp - DateTime.UtcNow.Date;
            if (timeSpan.Days <= 30)
            {

                EmailService.SendEmailAsync(deviceName, deviceExp.Date, log);
            }

        }
    }
}
