using ParkLot.Models.DAL;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ParkLot.Models
{
    public class Borrow
    {
        DateTime startDate;
        DateTime endDate;
        DateTime startTime;
        DateTime endTime;
        string parkingName;
        string email;
        int id;
        int status;

        public DateTime StartDate { get => startDate; set => startDate = value; }
        public DateTime EndDate { get => endDate; set => endDate = value; }
        public DateTime StartTime { get => startTime; set => startTime = value; }
        public DateTime EndTime { get => endTime; set => endTime = value; }
        public string ParkingName { get => parkingName; set => parkingName = value; }
        public string Email { get => email; set => email = value; }
        public int Id { get => id; set => id = value; }
        public int Status { get => status; set => status = value; }

        public List<string> BorrowParkingName(string email) //שליפת שם חניה של משתמש לפי מייל
        {
            DBservices dbs = new DBservices();
            return dbs.ReadPrkimgName(email);
        }

        public int insertBorrowU() //הכנסת השאלה
        {
            DBservices dbs = new DBservices();
            return dbs.InsertBorrow(this);
        }

        
        public int insertToAskFor(int idBorrow, int idRequest) //הכנסת מאץ
        {
            DBservices dbs = new DBservices();
            return dbs.InsertToAsk_for(idBorrow, idRequest);
        }

        public List<object> DesirableB(DateTime desiredDate, DateTime startTime, DateTime endTime, string email, int idB) //שליפת שם חניה של משתמש לפי מייל
        {
            DBservices dbs = new DBservices();
            List<object> results = dbs.AvailabilityCheck(desiredDate, startTime, endTime, email, idB);
            return results;
        }

        public List<Borrow> UserBorrows(string mail)  //שליפת השאלות לפי משתמש
        {
            DBservices dbs = new DBservices();
            List<Borrow> UBorrows;
            try
            {
                 UBorrows = dbs.GetUserBorrows(mail);  //שליפת השאלות לפי משתמש                
            }
            catch (Exception)
            {
                throw new Exception("in GetUserBorrows");
            }

            for (int i = 0; i < UBorrows.Count; i++) // בודקת כל השאלה עם היא מלאה או לא
            {
                List<object> approvedMatches = new List<object>(); //רשימה של מאצים שאושרו בלבד
                approvedMatches = dbs.GetApproveMatch(UBorrows[i].id);//מביאה את כל המאצים שיש להשאלה

                if (approvedMatches.Count > 0) //רק אם רשימת מאצים מאושרים מלאה
                {
                    TimeSpan gap;   //משתנה עזר רק זמן
                    bool isUsed = false; //משתנה עזר כדי לדעת אם יש רווח של שעה

                    List<dynamic> approvedMatchesDynamic = approvedMatches.Cast<dynamic>().ToList();//הופך את הרשימה לדינאמית
                    approvedMatches = approvedMatchesDynamic.OrderBy(dt => dt.RequestStartDate).ToList(); //מסדר את הבקשות לפי שעת התחלה 
                    //approvedMatches = approvedMatchesDynamic.Cast<object>().ToList();//מחזיר לרשימה של אובייקטים

                    // בדיקה אם יש רווח של שעה בין שעת ההתחלה של ההשאלה לבין שעת ההתחלה של הבקשה הראשונה
                    var approvedMatchFirst = (dynamic)approvedMatches[0];
                    gap = approvedMatchFirst.RequestStartTime - UBorrows[i].StartTime;
                    if (Math.Abs(gap.TotalHours) >= 1) //במידה ויש רווח של שעה לפחות
                    {
                        isUsed = true;
                    }

                    // בדיקה אם יש רווח של שעה בין הבקשות
                    for (int z = 1; z < approvedMatches.Count; z++)
                    {
                        var approvedmatch = (dynamic)approvedMatches[z];
                        var approvedmatchBefore = (dynamic)approvedMatches[z - 1];
                        gap = approvedmatch.RequestStartTime - approvedmatchBefore.RequestEndTime;//אם יש רווח של שעה בין הבקשה הקודמת לבאה
                        if (Math.Abs(gap.TotalHours) >= 1) //במידה ויש רווח של שעה לפחות
                        {
                            isUsed = true;
                        }
                    }

                    // בדיקה אם יש רווח של שעה בין שעת סיום של הבקשה האחרונה לבין שעת סיום של ההשאלה
                    var approvedMatchLast = (dynamic)approvedMatches[approvedMatches.Count - 1]; // Conversion to a dynamic object so that we can access the fields
                    gap = UBorrows[i].EndTime - approvedMatchLast.RequestEndTime;
                    if (Math.Abs(gap.TotalHours) >= 1)//במידה ויש רווח של שעה לפחות
                    {
                        isUsed = true;
                    }
                    DBservices dBservices = new DBservices();
                    //אחרי כל הבדיקות
                    if (isUsed == true)//אם אחד התנאי יתקיים אז יש מקום בהשאלה
                    {
                        if (UBorrows[i].status != 0) //אם הסטטוס של ההשאלה מלאה נעדכן אותו בבסיס נתונים
                        {
                            dBservices.UpdateBorrows(UBorrows[i].id,0); //עדכון סטטוס לפנוי
                        }
                        UBorrows[i].status = 0; //יש מקום
                        
                    }
                    else //אם אף תנאי לא התקיים אין מקום בהשאלה
                    {
                        if (UBorrows[i].status != 1) //אם הסטטוס של ההשאלה ריק נעדכן אותו בבסיס נתונים
                        {
                            dBservices.UpdateBorrows(UBorrows[i].id, 1); //עדכון סטטוס ללמלא
                        }
                        UBorrows[i].status = 1; //מלא

                    }
                }

            }

            return UBorrows;
        }

        public int UpdateBorrow(Borrow Borrow)  //עדכון פרטי הזמנה
        {
            List<object> CanceledMatch = GetMatchFromDB(Borrow.Id);//מוציאה את כל המאצים שיש להשאלה
            if (CanceledMatch != null) //במידה ויש מאציץ
            {
                for (int i = 0; i < CanceledMatch.Count; i++)
                {
                    var match = (dynamic)CanceledMatch[i]; // המרה לאוביקט דינמי על מנת שנוכל לגשת לשדות
    
                    TimeSpan borrowStartTime = Borrow.startTime.TimeOfDay;  //המרה לפורמט מתאים לשם השוואה
                    TimeSpan borrowEndTime = Borrow.endTime.TimeOfDay;
                    TimeSpan RequestStartTime = match.RequestEndTime.TimeOfDay;
                    TimeSpan RequestEndTime = match.RequestStartTime.TimeOfDay;

                    //לבדוק עם כל בקשה עדיין בתוך הטווח שעות של ההשאלה
                    //אם היא לא בטווח וגם אישרו אותה
                    //אז נשלח מייל למשתמש שבוטל
                    if (match.BorrowEndDate < match.RequestStartDate ||match.BorrowStartDate > match.RequestEndDate ||
                        (match.BorrowStartDate == match.RequestEndDate && borrowStartTime > RequestEndTime) ||
                        (match.BorrowEndDate == match.RequestStartDate && borrowEndTime < RequestStartTime))
                    {
                        DBservices dbsDeleteMa = new DBservices();
                        dbsDeleteMa.DeleteMatch(match.BorrowId, match.RequestId); //מוחקים את המאץ מטבלת הקשר
                        if (match.status == 1) //אם כבר אושר אז נעדכן פרטים ואת המשתמש
                        {
                            dbsDeleteMa.UpdateRequestStatus(match.RequestId, 0); //מעדכנים בקשה לסטטוס אפס
                            Email E = new Email();
                            E.deleteEmail(match);  //שליחת מייל למשתמש המבקש שההשאלה בוטלה
                        } 
                    }                    
                }
                DBservices dbsPUT2 = new DBservices();
                return dbsPUT2.UpdateBorrows(Borrow); //מעדכנים השאלה
            }
            else //במידה ואין מאץ
            {
                DBservices dbsPUT = new DBservices();
                return dbsPUT.UpdateBorrows(Borrow); //מעדכנים השאלה
            }
        }

        public int DeleteBorrow(int id, string mail) //מחיקת השאלה
        {
            List<object> CanceledMatch = GetMatchFromDB(id);//מוציאה את כל המאצים שיש להשאלה
            if (CanceledMatch != null) //במידה ויש מאציץ
            {
                for (int i = 0; i < CanceledMatch.Count; i++)
                {
                    var match = (dynamic)CanceledMatch[i]; // המרה לאוביקט דינמי על מנת שנוכל לגשת לשדות
                    DBservices dbsDeleteMa = new DBservices();
                    dbsDeleteMa.DeleteMatch(match.BorrowId, match.RequestId); //מוחקים את המאץ מטבלת הקשר
                    if (match.status == 1) // במידה והמאץ אושר 
                    {
                        DBservices dbsupdate = new DBservices();
                        dbsupdate.UpdateRequestStatus(match.RequestId, 0); //מעדכנים בקשה לסטטוס אפס
                        Email E = new Email();
                        E.deleteEmail(match);  //שליחת מייל למשתמש המבקש שההשאלה בוטלה
                    }
                
                }
                DBservices dbsDE2 = new DBservices();
                return dbsDE2.DeleteBorrows(id, mail); //מוחקים השאלה
            }
            else   
            {
                DBservices dbsDE = new DBservices();
                return dbsDE.DeleteBorrows(id, mail); //מוחקים השאלה
            }

        }

        public List<object> GetMatchFromDB(int IdBorrow) //שליפת כל המאצים כלומר האנשים שהזמינו את החניה
        {
            List<object> CanceledMatch = new List<object>();
            DBservices dbs = new DBservices();
            CanceledMatch = dbs.GetMatch(IdBorrow); //מוציאה את כל המאצים שיש להשאלה
            return CanceledMatch;   
        }
    }
}
