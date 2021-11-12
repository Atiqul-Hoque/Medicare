using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;

namespace CapstoneMedicareNotification.Service
{
    class EmailService
    {
        

        public static async void SendEmailAsync(string deviceName, DateTime deviceExp, ILogger log)
        {
            try
            {
                var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
                string mailTo = config["MAIL_TO"];
                string mailFrom = config["MAIL_FROM"];
                string apiKey = config["SENDGRID_APIKEY"];
                log.LogInformation($"Email Notification to be sent to ." + mailTo);

                string body = "<br><br><table border=\"2\"><tr><th>Device Name</th><th>Expiry Date</th></tr><td>" + deviceName + "</td><td>" + deviceExp.ToShortDateString() + "</td></tr></table>";
                body = body + "<p><p>Please login to the PRODA Account and Extend B2B device.</p></p>";


                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(mailFrom);
                var subject = "Proda Device " + deviceName + " to be Expired .";
                var to = new EmailAddress(mailTo);
                var plainTextContent = "Device Name " + deviceName + "will be expired on" + deviceExp.ToShortDateString() + "\n" + "Kindly go to your PRODA Account and Extend B2B device.";
                var htmlContent = body;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                log.LogInformation($"Email Notification sent.");
            }
            catch (Exception e)
            {
                log.LogInformation($"Email Notification Error."+e.Message);
            }

        }
      

    }
}
