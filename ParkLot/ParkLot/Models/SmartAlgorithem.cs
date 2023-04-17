
using ParkLot.Models.DAL;
using Microsoft.AspNetCore.HttpOverrides;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ParkLot.Models
{
    public class SmartAlgorithem
    {
     
        public List<object> getBorrowDetails(int idBorrow) //מחזיר את כל הבקשות המתאימות להשאלה
        {
            DBservices dbs = new DBservices();
            Borrow selectBorrow = dbs.getBorrowD(idBorrow); //שולפת את ההשאלה שעליה לחץ
            int BuildingID = dbs.GetBuildingID(idBorrow);
            List<Request> RequestList = dbs.getSmartRequest(idBorrow, BuildingID); //מחזיר רשימה של כל הבקשות המתאימות להשאלה
           
            List<List<Request>> AllPossibleCombinations = new List<List<Request>>(); //רשימה של רשימות
            AllPossibleCombinations = GenerateRequestCombinations(selectBorrow, RequestList); //מחזיר את כל הקומבינציות האפשריות של הבקשות
            
            List<Request> selectedCombination= new List<Request>();
            selectedCombination= GetCombinationWithMostUtilization(AllPossibleCombinations, selectBorrow); //מחזיר את הקומבינציה שמנצלת הכי הרבה זמן מההשאלה
            List<object> FinalCombination = new List<object>(); //קובינציה שנוסיף לה את שמות המבקשים
            if (selectedCombination != null)  //רק אם חזרה רשימה
            {
                for (int i = 0; i < selectedCombination.Count; i++)
                {
                    DBservices dbs2 = new DBservices();
                    object requestWithName = dbs2.GetNameRequest(selectedCombination[i].Email, selectedCombination[i].Id); //תחזיר לנו לכל בקשה את שם המבקש
                    FinalCombination.Add(requestWithName);
                }
            } 
            return FinalCombination;  //מחזיר את הקומבינציה הכי טובה
        }


        //מחזירה רשימה של כל השילובים האפשריים של בקשות שניתן להכיל בתוך ההשאלה
        public List<List<Request>> GenerateRequestCombinations(Borrow selectedBorrow, List<Request> requestList)
        {
            List<List<Request>> combinations = new List<List<Request>>(); //רשימת הרשימות- רשימה גדולה המכילה את הרשימות הקטנות
            List<Request> currentCombination = new List<Request>(); //רשימה בלבד-קטנה
            int index = 0;
            GenerateCombinationsRecursive(selectedBorrow, requestList, combinations, currentCombination, index);
            return combinations;
        }

        //פונקציה רקורסיבית שיוצרת את כל השילובים האפשריים
        private void GenerateCombinationsRecursive(Borrow selectedBorrow, List<Request> requestList, List<List<Request>> combinations, List<Request> currentCombination, int index)
        {
            //אם בקומבינציה הנוכחית יש בה משהו ובנוסף עומדת בזמני ההשאלה
            if (currentCombination.Count > 0 && IsCombinationValid(selectedBorrow, currentCombination))  
            {
                //מכניסה את הרשימה הקטנה לרשימה הגדולה
                combinations.Add(new List<Request>(currentCombination));
            }
            //רץ על כל הבקשות האפשריות שחזרו אלינו
            //כל פעם מתחיל לרוץ מהאינדקס הבא ככה אין כפילויות
            for (int i = index; i < requestList.Count; i++)
            {
                currentCombination.Add(requestList[i]); //מוסיף בקשה לרשימה הקטנה
                GenerateCombinationsRecursive(selectedBorrow, requestList, combinations, currentCombination, i + 1);
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }

        //בודקת אם שילוב של בקשות תקף עבור ההשאלה שנבחרה
        //מקבל השאלה ורשימה קטנה
        private bool IsCombinationValid(Borrow selectedBorrow, List<Request> combination)
        {
            //סידור תאריך וזמן של השאלה במשתנה אחד
            DateTime borrowStart = selectedBorrow.StartDate.Date + selectedBorrow.StartTime.TimeOfDay;
            DateTime borrowEnd = selectedBorrow.EndDate.Date + selectedBorrow.EndTime.TimeOfDay;

            // Check if any request in the combination overlaps with another request
            for (int i = 0; i < combination.Count; i++)
            {
                //סידור תאריך וזמן של בקשה במשתנה אחד
                DateTime requestStart1 = combination[i].StartDate.Date + combination[i].StartTime.TimeOfDay;
                DateTime requestEnd1 = combination[i].EndDate.Date + combination[i].EndTime.TimeOfDay;

                for (int j = i + 1; j < combination.Count; j++) 
                {
                    //סידור תאריך וזמן של בקשה במשתנה אחד
                    DateTime requestStart2 = combination[j].StartDate.Date + combination[j].StartTime.TimeOfDay;
                    DateTime requestEnd2 = combination[j].EndDate.Date + combination[j].EndTime.TimeOfDay;
                    //אם יש חפיפה בין שתי בקשות בקומבינציה הנתונה אז היא מחזירה שקר
                    if ((requestStart1 >= requestStart2 && requestStart1 < requestEnd2) ||
                        (requestEnd1 > requestStart2 && requestEnd1 <= requestEnd2) ||
                        (requestStart1 < requestStart2 && requestEnd1 > requestEnd2))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //מוצא מי הקומבינציה שמנצלת את הזמן הכי טוב
        public List<Request> GetCombinationWithMostUtilization(List<List<Request>> combinations, Borrow selectedBorrow)
        {
            List<Request> maxUtilizationCombination = null;
            double maxUtilizationTime = 0;
            //עבור כל רשימה קטנה בתוך הרשימה הגדולה שמכילה רשימות
            foreach (var combination in combinations)
            {
                double totalUtilizationTime = 0;
                foreach (var request in combination) //עבור כל בקשה ברשימה הקטנה
                {
                    //סידור תאריך וזמן של בקשה במשתנה אחד
                    DateTime requestStart = request.StartDate.Date + request.StartTime.TimeOfDay;
                    DateTime requestEnd = request.EndDate.Date + request.EndTime.TimeOfDay;
                    //חישבו הזמן הכולל של הרשימה הקטנה
                    double requestUtilizationTime = (requestEnd - requestStart).TotalHours;
                    totalUtilizationTime += requestUtilizationTime;
                }
                //סידור תאריך וזמן של השאלה במשתנה אחד
                DateTime borrowStart = selectedBorrow.StartDate.Date + selectedBorrow.StartTime.TimeOfDay;
                DateTime borrowEnd = selectedBorrow.EndDate.Date + selectedBorrow.EndTime.TimeOfDay;
                double borrowTotalTime = (borrowEnd - borrowStart).TotalHours; //חישוב זמן כולל של ההשאלה

                // Check if this combination uses the most time so far
                if (totalUtilizationTime > maxUtilizationTime && totalUtilizationTime <= borrowTotalTime)
                {
                    maxUtilizationTime = totalUtilizationTime;
                    maxUtilizationCombination = combination;
                }
            }
            return maxUtilizationCombination;
        }

        public int UpdateToSmartAlgorithem(int idBorrow) //בחירת שיבוץ חכם בעקבות כך -עדכון משתמשים נעשה בהתאם למצבם
        {
            List<object> BestAlgorithem = new List<object>();
            BestAlgorithem = getBorrowDetails(idBorrow);
            
            List<object> Match = new List<object>();
            DBservices dbs = new DBservices();
            Match = dbs.GetMatch(idBorrow);//הוצאת מאצ'ים גם מי שמאושר וגם מי שממתין
            //בדיקה ראשונה
            for (int i = 0; i < Match.Count; i++) //עוברים על כל המאצים הקיימים
            {
                var Matching = (dynamic)Match[i];
                if (Matching.status == 1)//אם אושר
                {
                    int counter = 0;
                    for (int j = 0; j < BestAlgorithem.Count; j++) //עוברים על השיבוץ החכם
                    {
                        var Smart = (dynamic)BestAlgorithem[j];
                        if (Smart.RequestId == Matching.RequestId)//אם קיים באלגוריתם חכם
                        {
                            counter++;//סימן שהוא באלגוריתם חכם
                        }
                    }
                    if (counter == 0)// לא קיים באלגוריתם החכם ולכן צריך לבטל אותו ולשלוח מייל
                    {
                        DBservices dbsUpdate = new DBservices();
                        dbsUpdate.UpdateAsk_forStatus(Matching.BorrowId, Matching.RequestId, 0);//עדכון הסטטוס של המאצ' ל-0
                        dbsUpdate.UpdateRequestStatus(Matching.RequestId, 0);//עדכון הבקשה ל-0 כלומר לא מאושרת
                        Email E = new Email();
                        E.deleteEmail(Matching);//שליחת מייל שהתבטל המאצ' 
                    }
                }
            }
            //בדיקה שנייה לאחר שמי שצריך להישאר באלגוריתם נשאר
            //נבדוק מי הדיירים החדשים שצריכים להתעדכן וקבל מייל שיש להם התאמה
            for (int j = 0; j < BestAlgorithem.Count; j++) //עוברים על השיבוץ החכם
            {
                var Smart = (dynamic)BestAlgorithem[j];
                if (Smart.RequestStatus == 0) //בודקת מי מתוך האלגוריתם החכם עדיין לא אושר ומאשרת
                {
                    //בדיקה אם לא קיימת בטבלת מאץ אז צריך להכניס
                    DBservices dBCheck = new DBservices();
                    var CheckedMatch = dBCheck.GetcheckMatch(idBorrow, Smart.RequestId);//לאחר שקיבלתי את האלגוריתם בדיקה אם קיים במאצ'
                    if (CheckedMatch == 0) // אם אין לי אובייקט המכיל את המאץ
                    {
                        dBCheck.InsertToAsk_for(idBorrow, Smart.RequestId); // מוסיף את המאצ' לטבלת המאצ'ים לאחר אישור האלגוריתם חכם
                    }

                    //לאחר שקיימת בטבלת מאץ נשנה סטטוס ל1 גם בטבלת מאץ וגם בטבלת בקשות
                    dBCheck.UpdateAsk_forStatus(idBorrow, Smart.RequestId, 1);//עדכון המאץ' שיהיה מאושר
                    dBCheck.UpdateRequestStatus(Smart.RequestId, 1);//עדכון הבקשה שהסטטוס שלה מאושר
                    Email E = new Email();  //נשלח מייל שבקשה אושרה
                    E.updateRequestApproved(Smart);

                }
            }
            return 1;
        }

    }
}
    
                
