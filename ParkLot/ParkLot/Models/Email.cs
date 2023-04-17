using ParkLot.Models.DAL;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text.Json;

namespace ParkLot.Models
{
    public class Email
    {
       
        public int SendEmail(string email,string strSubject,string strBody)//שליחת קוד בניין למשתמש שנרשם
        {
            string from = "parkinglotprojct@gmail.com";
            string password = "hlul ahhi lqvu cylu"; // קבלת סיסמה מהגדרות של אימייל
            string host = "smtp.gmail.com"; // replace with your SMTP server address

            MailMessage message = new MailMessage();
            message.From = new MailAddress("parkinglotprojct@gmail.com"); // replace with a valid email address
            message.To.Add(email);
            message.Subject =strSubject;
            message.Body = strBody;

            SmtpClient smtp = new SmtpClient(host);
            smtp.Port = 587; // replace with your SMTP server port number
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // email is sent through the network
            smtp.Credentials = new NetworkCredential(from, password);
            smtp.EnableSsl = true;
            try
            {
                smtp.Send(message);
                Console.WriteLine("Email sent successfully!");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send email. Error message: " + ex.Message);
                return 0;
            }
        }


        public int RegistrationEmail(string sendEmail, string BildingP)  //שליחת מייל לאחר הרשמה לאתר
        {
            string strsubject = "קוד בניין";
            string strbody = "ההרשמה בוצעה בהצלחה קוד הבניין שלך הוא: " + BildingP;
            return SendEmail(sendEmail, strsubject, strbody);
        }


        public int deleteEmail(object data) //שליחת מייל למבקש החנייה שהיא בוטלה
        {
            var match = (dynamic)data; // המרה לאובייקט דינמי כדי שנוכל לגשת לשדות
            string borrowStartDateString = match.BorrowStartDate.ToString("dd/MM/yyyy");
            string borrowEndTimeString = match.BorrowEndTime.ToString("hh\\:mm");
            string borrowStartTimeString = match.BorrowStartTime.ToString("hh\\:mm");
            string strsubject = "ביטול חניה -ParkLot";
            string str = "לצערינו השאלה מספר: "+match.BorrowId;
            str +="\n";
            str +="בתאריך: "+ borrowStartDateString;
            str+= "\n";
            str += "בין השעות: " + borrowStartTimeString + "-" + borrowEndTimeString;
            str += "\n";
            str += "מבוטלת, אך הבקשה שלך עדיין מופיעה ברשימת הבקשות הכלליות של הבניין";
            str += "\n";
            str += "אנא כנס לאתר לבדוק אם קיימות חניות אחרות שזמינות בשבילך, יום טוב";
            return SendEmail(match.RMail, strsubject,str);
        }

        public int RequestEmail(string EmailAddress,int idBorrow)  //שליחת מייל למשאיל החנייה שיש לו בקשה חדשה
        {
            string strsubject = "בקשה חדשה -ParkLot";
            string strbody = "ממתינה לך בקשה חדשה לאישור באתר עבור השאלה מספר: " + idBorrow;
            return SendEmail(EmailAddress, strsubject, strbody);
        }


        public int updateRequestApproved(object data)  //שליחת מייל שבקשה אושרה
        {
            var match = (dynamic)data; // המרה לאובייקט דינמי כדי שנוכל לגשת לשדות
            string RequestStartDateString = match.RequestStartDate.ToString("dd/MM/yyyy"); //בדיקה
            string RequestEndTimeString = match.RequestStartTime.ToString("hh:mm:ss tt");
            string RequestStartTimeString = match.RequestEndTime.ToString("hh:mm:ss tt");
            string strsubject = "בקשה אושרה -ParkLot";
            string str = "בקשה מספר: " + match.RequestId+" אושרה";
            str += "\n";
            str += "בתאריך: " + RequestStartDateString;
            str += "\n";
            str += "בין השעות: " + RequestStartTimeString + "-" + RequestEndTimeString;
            str += "\n";
            str += "יום טוב, צוות Park-Lot";
            return SendEmail(match.RMail, strsubject, str);
        }

        public int GettingRating(Rating rate)  //שליחת מייל על דירוג שקיבלו
        {
            DBservices DB = new DBservices();
            Borrow borrow=new Borrow();
            borrow = DB.getBorrowD(rate.Id_Borrow);  //שליפת פרטי השאלה
            Request request = new Request();
            request = DB.getRequest(rate.Id_Request); //שליפת פרטי בקשה
            string strsubject = "דירגו אותך -ParkLot";
            User user= new User();
            user = DB.ReadUser(rate.Email_giver);
            string str = "קיבלת דירוג מ: " + user.FirstName + " " +user.FamilyName;
            str += "\n";
            str += "על השאלה בתאריך: " + borrow.StartDate.Date.ToString("dd/MM/yyyy"); ;
            str += "\n";
            str += "בין השעות: " + borrow.StartTime.TimeOfDay + "-" + borrow.EndTime.TimeOfDay;
            str += "\n";
            str += "ובקשה בשעות: " + request.StartTime.TimeOfDay + "-" + request.EndTime.TimeOfDay;
            str += "\n";
            str += "דירגו אותך בציון של: " + rate.Grade;
            str += "\n";
            if (rate.Notes!="") /* במידה ויש הערות שניתנו*/
            {
                str += "הערות שקיבלת: " + rate.Notes;
                str += "\n";
            }
            str += "יום טוב, צוות Park-Lot";
            return SendEmail(rate.Email_reciver, strsubject, str);
        }

    }


}
   