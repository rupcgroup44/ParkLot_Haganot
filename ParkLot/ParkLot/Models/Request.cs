using ParkLot.Models.DAL;
namespace ParkLot.Models
{
    public class Request
    {
        int id;
        DateTime startDate;
        DateTime endDate;
        DateTime startTime;
        DateTime endTime;
        int status;
        string email;

        private dynamic selectedMatch;
        private DateTime selectedMatchET;
        private DateTime selectedMatchST;

        public int Id { get => id; set => id = value; }
        public DateTime StartDate { get => startDate; set => startDate = value; }
        public DateTime EndDate { get => endDate; set => endDate = value; }
        public DateTime StartTime { get => startTime; set => startTime = value; }
        public DateTime EndTime { get => endTime; set => endTime = value; }
        public int Status { get => status; set => status = value; }
        public string Email { get => email; set => email = value; }


        public int InsertRequestU() //הכנסת בקשה לדף הכללי של הבקשות
        {
            DBservices dbs = new DBservices();
            return dbs.InsertRequest(this);
        }

        public int UpdateReqForBorrow(int idBorrow,int idRequest) //אישור בקשה להשאלה במידה וניתן
        {
            Borrow borrow = new Borrow();   
            List<object> CheckMatch = borrow.GetMatchFromDB(idBorrow);//מוציאה את כל המאצים שיש להשאלה
            for (int i = 0; i < CheckMatch.Count; i++)
            {
                var lookMatch = (dynamic)CheckMatch[i]; // המרה לאוביקט דינמי על מנת שנוכל לגשת לשדות
                if (lookMatch.RequestId== idRequest) //חיפוש הבקשה שרצו לאשר
                {
                    selectedMatch= lookMatch; //שמירת הבקשה שרצו לאשר
                    selectedMatchST = lookMatch.RequestStartDate.Date + lookMatch.RequestStartTime.TimeOfDay; //שמירת זמן התחלה בפורמט מתאים להשוואה
                    selectedMatchET = lookMatch.RequestEndDate.Date + lookMatch.RequestEndTime.TimeOfDay;  //שמירת זמן סיום בפורמט מתאים להשוואה
                }
            }
            if (CheckMatch != null) //במידה ויש מאציצים
            {
                List<object> ConfirmedMatch = new List<object>();
                for (int i = 0; i < CheckMatch.Count; i++)//מי מהם אושר
                {
                    var match = (dynamic)CheckMatch[i]; // המרה לאוביקט דינמי על מנת שנוכל לגשת לשדות
                    if (match.status==1) //סטטוס של המאצים
                    {
                        ConfirmedMatch.Add(match);  //מכניסה לרשימה חדשה של מאושרים בלבד
                    }
                }
                if (ConfirmedMatch != null) //אם הרשימה לא ריקה
                {
                    //בדיקה האם יש מקום לבקשה שרצו לאשר בהשאלה
                    for (int i = 0; i < ConfirmedMatch.Count; i++)
                    {
                        var match = (dynamic)ConfirmedMatch[i];
                        DateTime reqST = match.RequestStartDate.Date + match.RequestStartTime.TimeOfDay; //שמירת זמן התחלה בפורמט מתאים להשוואה
                        DateTime reqET = match.RequestEndDate.Date + match.RequestEndTime.TimeOfDay;  //שמירת זמן סיום בפורמט מתאים להשוואה
                        if (selectedMatchST < reqET && selectedMatchET > reqST)  //עובד גם ברגיל גם משמרת לילה
                        {
                            return 0; //אי אפשר להכניס אותו הוא חופף עם בקשות אחרות
                        }
                    }
                }else return UpdateUser(idBorrow, idRequest, selectedMatch, 1); //ניתן לאשר את הבקשה לכן
            }
            else //במידה ואין לו מאצים כלומר הוא פנוי לגמרי
            {
                return UpdateUser(idBorrow, idRequest, selectedMatch, 1); //ניתן לאשר את הבקשה לכן
            }
            return UpdateUser(idBorrow, idRequest, selectedMatch, 1); //ניתן לאשר את הבקשה לכן
        }

        public int UpdateCancealedForBorrow(int idBorrow, int idRequest, int status)
        {
            Borrow borrow = new Borrow();  //בשביל שליחת המייל
            List<object> CheckMatch = borrow.GetMatchFromDB(idBorrow);//מוציאה את כל המאצים שיש להשאלה
            for (int i = 0; i < CheckMatch.Count; i++)
            {
                var lookMatch = (dynamic)CheckMatch[i]; // המרה לאוביקט דינמי על מנת שנוכל לגשת לשדות
                if (lookMatch.RequestId == idRequest) //חיפוש הבקשה שרצו לבטל
                {
                    selectedMatch = lookMatch; //שמירת הבקשה שרצו לבטל
                }
            }
            return UpdateUser(idBorrow, idRequest, selectedMatch, status); //שליחה לשם עדכון הטבלאות ושליחת המייל
        }

        public int UpdateUser(int idBorrow,int idRequest, object selectedMatch, int status) //עדכון בקשה וגם טבלת מאץ ושליחת מייל
        {
            DBservices DBreq = new DBservices();
            DBreq.UpdateAsk_forStatus(idBorrow, idRequest, status); //נעדכן טבלת מאצים שהסטטוס אחד
            DBservices dbsupdate = new DBservices();
            Email E = new Email();
            if (status == 1)
            {
                E.updateRequestApproved(selectedMatch);  //שליחת מייל למבקש שהבקשה התקבלה
            }
            else E.deleteEmail(selectedMatch);
            return dbsupdate.UpdateRequestStatus(idRequest, status); //מעדכנים בקשה לסטטוס אחד;
        }
    }
}
