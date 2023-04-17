using ParkLot.Models.DAL;

namespace ParkLot.Models
{
    public class Rating
    {
        string email_giver;
        string email_reciver;
        int id_Borrow;
        int id_Request;    
        int grade;
        string notes;

        public string Email_giver { get => email_giver; set => email_giver = value; }
        public string Email_reciver { get => email_reciver; set => email_reciver = value; }
        public int Id_Borrow { get => id_Borrow; set => id_Borrow = value; }
        public int Id_Request { get => id_Request; set => id_Request = value; }
        public string Notes { get => notes; set => notes = value; }
        public int Grade { get => grade; set => grade = value; }

        public void GetRate(Rating R) //חישוב הדירוג של המשתמש
        {
            DBservices dbs = new DBservices();
            int Rating= dbs.GetUserRate(R.email_reciver);//חישוב דירוג ממוצע
            UpdateRating(R.email_reciver, Rating);//הפעלת פונקצית עדכון הדירוג.
        }

        public int UpdateRating(string mail, int rate )//עדכון הדירוג של המשתמש
        {
            DBservices dbs = new DBservices();
            return dbs.UpdateUserRate(mail,rate);
        }

        public List<object>GetArchiveData(string mail)//הוצאת הארכיון של המשתמש
        {
            DBservices dbs = new DBservices();
            return dbs.GetArchive(mail);
            
        }


        public int InsertRating()//הכנסת דירוג
        {
            DBservices dbs = new DBservices();
            int num= dbs.InserRate(this);
            GetRate(this);
            Email email= new Email();
            email.GettingRating(this);
            return num;

        }
        
    }


}
