using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoboReplicate
{
    class SMTP
    {
        public static void Send(string Host, int Port, bool SSL, string Username, string Password, string Domain, string From, string To, string Subject, string Message)
        {
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Host, Port);
            client.EnableSsl = SSL;
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(Username, Password, Domain);
            client.DeliveryFormat = System.Net.Mail.SmtpDeliveryFormat.International;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(From, To, Subject, Message);
            client.Send(msg);
        }

        public static void SendWithAttachment(string Host, int Port, bool SSL, string Username, string Password, string Domain, string From, string To, string Subject, string Message, string attachmentPath)
        {
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Host, Port);
            client.EnableSsl = SSL;
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(Username, Password, Domain);
            client.DeliveryFormat = System.Net.Mail.SmtpDeliveryFormat.International;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(From, To, Subject, Message);
            msg.Attachments.Add(new System.Net.Mail.Attachment(attachmentPath));
            client.Send(msg);
        }
    }
}
