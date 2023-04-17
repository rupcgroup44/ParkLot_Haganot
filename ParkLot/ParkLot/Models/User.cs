using ParkLot.Models.DAL;

namespace ParkLot.Models
{
    public class User
    {
        string email;
        string firstName;
        string familyName;
        string city;
        string street;
        string password;
        int coins;
        int idBuilding;
        string buildingCode;
        int buildingNumber;

        public string Email { get => email; set => email = value; }
        public string FirstName { get => firstName; set => firstName = value; }
        public string FamilyName { get => familyName; set => familyName = value; }
        public string City { get => city; set => city = value; }
        public string Street { get => street; set => street = value; }
        public string Password { get => password; set => password = value; }
        public int Coins { get => coins; set => coins = value; }
        public int IdBuilding { get => idBuilding; set => idBuilding = value; }
        public string BuildingCode { get => buildingCode; set => buildingCode = value; }
        public int BuildingNumber { get => buildingNumber; set => buildingNumber = value; }


        public User login(string email)
        {
            DBservices dbs = new DBservices();
            return dbs.ReadUser(email);
        }

        public int InsertUser(User user, string building_code)   
        {

            DBservices dbs = new DBservices();
            int idBuild= dbs.ReadByAddress(user.city,user.street, user.buildingNumber);

            if(idBuild!=0)//אם קיים בניין כזה
            {
            return dbs.InsertUser(user, idBuild);//תכניס לי את המשתמש
            }
            else//אם לא קיים בניין
            {
                dbs.InsertBuilding(building_code);//הכנסת בניין חדש
                int id=dbs.ReadBuildingId(building_code);//קבלת ה-ID  של הבניין החדש
                return dbs.InsertUser(user,id);//הכנסת יוזר חדש עם בניין 
            } 

        }

        public int insertParking(string[] parkingSpots, string Email,string building_code)
        {
            DBservices dbs = new DBservices();
            int BuildingId = dbs.ReadBuildingId(building_code); // הוצאה מספר בניין שאני שייך 
            int CheckedParkingName;
            for (int i = 0; i < parkingSpots.Length-1;i++) {//ריצה על החניות שאני רוצה להכניס
                if (parkingSpots[i]!= null) //כל עוד יש לי במערך שם חניה
                {
                    CheckedParkingName = dbs.GetcheckParkingName(BuildingId, parkingSpots[i]); //בדיקה האם שם חניה ספציפי כבר קיים בבניין
                    if (CheckedParkingName == 1) // לא קיים שם חניה בבניין
                    {
                        int ANS = dbs.DeleteUser(Email);//מוחק את המשתמש
                        if (ANS == 1)//הצליח למחוק את המשתמש
                        { return 0; }
                    }
                } 
            }
            return dbs.InsertParking(parkingSpots, Email); // הוספה לטבלה של חניות עם שמות חניות תקינות 
        }
       
        public int insertPhone(string[] phoneNum, string email)
        {
            DBservices dbs = new DBservices();
            return dbs.InsertPhoneNumber(phoneNum, email);

        }


    }
}
