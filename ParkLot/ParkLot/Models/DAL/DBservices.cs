using System.Data.SqlClient;
using System.Data;
using System.Reflection.Metadata;
using ParkLot.Models;
using System.Globalization;
using System.Data.Common;

namespace ParkLot.Models.DAL
{
    public class DBservices
    {
        public SqlDataAdapter da;
        public DataTable dt;

        public SqlConnection connect(String conString) //appsettings.jsonשהוגדר לו ב ConnectionStringחיבור בין הבסיס נתונים לויזול לפי 
        {
            // read the connection string from the configuration file
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return con;
        }

        public User ReadUser(string emailU) //קריאה למשתמש לפי מייל    
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateUserDetailByEmail("SpLogin", con, emailU);  //קראנו למשתמש לפי מייל
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                User user = new User();
                while (dataReader.Read()) //במידה ויש משתמש מרשימת המשתמשים שהמייל זהה
                {
                    user.Email = dataReader["email"].ToString();
                    user.FirstName = dataReader["firstName"].ToString();
                    user.FamilyName = dataReader["familyName"].ToString();
                    user.City = dataReader["city"].ToString();
                    user.Street = dataReader["street"].ToString();
                    user.Password = dataReader["password"].ToString();
                    user.Coins = Convert.ToInt32(dataReader["coins"]);
                    user.IdBuilding = Convert.ToInt32(dataReader["idBuilding"]);
                    user.BuildingCode = dataReader["buildingCode"].ToString();
                }
                return user;

            }
            catch (SqlException ex)
            {
                // write to log
                Console.WriteLine("Error reading user from database: " + ex.Message);
                return null;
            }
            finally
            {

                if (con != null)
                {
                    con.Close();
                }
            }

        }
        private SqlCommand CreateUserDetailByEmail(String spName, SqlConnection con, string email)
        {

            SqlCommand cmd = new SqlCommand(); // create the command object---------------------------------------

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure


            cmd.Parameters.AddWithValue("@email", email);

            return cmd;
        }

        public int InsertUser(User user, int idBuilding) // הכנסה של משתמש
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            if (user == null)//אם מקבל יוזר ריק
            {
                throw new ArgumentException("The user object is invalid");//זורק חריגה שקיבל אובייקט ריק
            }
            cmd = CreateCommand("SpRegisterUser", con, user, idBuilding);     // create the command
            try
            {
                if (con.State != ConnectionState.Open)//אם החיבור לדאטהבייס סגור תזרוק חריגה
                {
                    throw new InvalidOperationException("The database connection is closed");//
                }
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (InvalidOperationException ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        private SqlCommand CreateCommand(String spName, SqlConnection con, User user, int id) //הכנסה משתמש
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@firstName", user.FirstName);
            cmd.Parameters.AddWithValue("@familyName", user.FamilyName);
            cmd.Parameters.AddWithValue("@city", user.City);
            cmd.Parameters.AddWithValue("@street", user.Street);
            cmd.Parameters.AddWithValue("@password", user.Password);
            cmd.Parameters.AddWithValue("@buildingNumber", user.BuildingNumber);
            cmd.Parameters.AddWithValue("@idBuilding", id);
            return cmd;
        }

        public int ReadByAddress(string city, string street, int buildingNumber) //קריאה לת.ז. בניין במידה וקיים משתמש אם אותם נתוני מגורים   

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(street))
            {
                throw new ArgumentException("City and street cannot be null or empty.");
            }

            if (buildingNumber < 1)
            {
                throw new ArgumentOutOfRangeException("Building number must be a positive integer.");
            }
            cmd = CreateReadIdbuildingSP("SpReadIdBuilding", con, city, street, buildingNumber);  //קראנו לת.ז. בניין לפי עיר, כתובת ומספר בניין

            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                User user = new User();
                while (dataReader.Read()) //במידה ויש משתמש מרשימת המשתמשים שהמגורים שלו זהים
                {
                    user.IdBuilding = Convert.ToInt32(dataReader["idBuilding"]);

                }
                return user.IdBuilding;

            }

            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        private SqlCommand CreateReadIdbuildingSP(String spName, SqlConnection con, string city, string street, int buildingNumber) //קבלת ID של הבניין
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@city", city);
            cmd.Parameters.AddWithValue("@street", street);
            cmd.Parameters.AddWithValue("@buildingNumber", buildingNumber);

            return cmd;
        }

        public int InsertBuilding(string BuildingCode) // הכנסה של קוד בניין
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateBuilding("SpRegisterBuilding", con, BuildingCode);    // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting building: " + ex.Message);
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        private SqlCommand CreateBuilding(String spName, SqlConnection con, string Code) //הכנסה בניין
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure
            cmd.Parameters.AddWithValue("@buildingCode", Code);

            return cmd;
        }

        public int ReadBuildingId(string CodeB) //הוצאת מספר בניין לפי קוד   

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateBuilding("SpReadBuildingId", con, CodeB);  //קראנו לכל המשתמשים
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                int IdBuilding = 0;
                while (dataReader.Read()) //במידה ויש משתמש מרשימת המשתמשים שהמייל זהה
                {
                    IdBuilding = Convert.ToInt32(dataReader["id"]);

                }
                return IdBuilding;

            }

            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }

        public int InsertParking(string[] parkingSpots, string Email) // הכנסה של שמות החניות 
        {
            int id = 1;
            SqlConnection con = null;
            SqlCommand cmd = null;
            int numEffected = 0;
            try
            {
                con = connect("myProjDB"); // create the connection

                // Loop through the parking spots and create a command for each one
                foreach (string parkingSpot in parkingSpots)
                {
                    if (!string.IsNullOrEmpty(parkingSpot)) //רק אם המקום חניה לא ריק נכניס את החנייה לבסיס נתונים
                    {
                        cmd = CreateInsertParkingCommand("SpRegisterParking", con, id, parkingSpot, Email); // create the command
                        numEffected += cmd.ExecuteNonQuery(); // execute the command and add to the total number of rows affected
                        id++;
                    }
                }
            }
            catch (Exception ex)
            {
                // write to log
                throw ex;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose(); // dispose the command object
                }
                if (con != null)
                {
                    con.Close(); // close the db connection
                    con.Dispose(); // dispose the connection object
                }
            }
            return numEffected;
        }
        private SqlCommand CreateInsertParkingCommand(String spName, SqlConnection con, int id, string parkingname, string email) //הכנסת חניה לטבלת חניות
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@parkingName", parkingname);
            cmd.Parameters.AddWithValue("@email_user", email);

            return cmd;
        }

        public int InsertPhoneNumber(string[] phoneNum, string email)  //הכנסת מספרי טלפונים של המשתמש
        {
            int id = 1;
            SqlConnection con = null;
            SqlCommand cmd = null;
            int numEffected = 0;

            try
            {
                con = connect("myProjDB"); // create the connection
                foreach (string phone in phoneNum)
                {
                    if (!string.IsNullOrEmpty(phone)) //רק אם המקום טלפון לא ריק נכניס את החנייה לבסיס נתונים
                    {
                        cmd = CreateInsertPhoneNumberCommand("SpInsertPhoneNumber", con, id, phone, email); // create the command
                        numEffected += cmd.ExecuteNonQuery(); // execute the command and add to the total number of rows affected
                        id++;
                    }
                }
            }
            catch (Exception ex)
            {
                // write to log
                throw ex;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose(); // dispose the command object
                }
                if (con != null)
                {
                    con.Close(); // close the db connection
                    con.Dispose(); // dispose the connection object
                }
            }

            return numEffected;
        }
        private SqlCommand CreateInsertPhoneNumberCommand(String spName, SqlConnection con, int id, string phone, string email) //הכנסת מספר טלפון
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@phoneNumber", phone);
            cmd.Parameters.AddWithValue("@email_user", email);

            return cmd;
        }

        public List<string> ReadPrkimgName(string email) //הוצאת שמות חניה לפי מייל   

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateUserDetailByEmail("SpReadParkingName", con, email);  //שליפת שמות חניה של משתמש לפי מייל
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                List<string> parkingName = new List<string>();
                while (dataReader.Read()) //במידה ויש משתמש מרשימת המשתמשים שהמייל זהה
                {
                    parkingName.Add(dataReader["parkingName"].ToString());

                }
                return parkingName;
            }
            catch (Exception ex)
            {
                // log the exception
                //Console.WriteLine($"Error code: {ex.ErrorCode}, Message: {ex.Message}");
                throw new Exception("Failed to read parking names", ex); // re-throw the exception with more specific information
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }

        public int InsertBorrow(Borrow borrow) //הזנת חניה פנויה- השאלה
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            int numEffected = 0;

            if (borrow.StartDate == borrow.EndDate) //הוכנס רק יום אחד
            {
                cmd = CreateInsertBorrow("CheckAndInsertBorrow", con, borrow); //להכניס כמו שזה
                try
                {
                    numEffected = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // write to log
                    throw (ex);
                }
            }
            else
            {
                if (borrow.StartDate.AddDays(1) == borrow.EndDate && borrow.EndTime < borrow.StartTime) //אם ההבדל הוא יום אחד וגם השעת התחלה גדולה מהסיום
                {
                    cmd = CreateInsertBorrow("CheckAndInsertBorrow", con, borrow);//להכניס כמו שזה
                    try
                    {
                        numEffected = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // write to log
                        throw (ex);
                    }
                }
                else //טווחים
                {
                    DateTime endDateForCurrentDay = borrow.EndDate; //שמירת תאריך סיום במשתנה עזר
                    for (DateTime date = borrow.StartDate; date <= endDateForCurrentDay; date = date.AddDays(1)) //במידה והוכנס טווח ימים
                    {
                        if (borrow.EndTime < borrow.StartTime)//אם הזין שבוע של משמרות לילה
                        {
                            borrow.StartDate = date;
                            borrow.EndDate = date.AddDays(1); //נקדם יום סיום באחד כי מסתיים בבוקר למחרת
                        }
                        else //הכנסה רגילה
                        {
                            borrow.StartDate = date;  //שינוי התחלה וסוף לכל יום בטווח
                            borrow.EndDate = date;
                        }
                        cmd = CreateInsertBorrow("CheckAndInsertBorrow", con, borrow);
                        try
                        {
                            numEffected += cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            // write to log
                            throw (ex);
                        }
                    }

                }

            }

            if (con != null)
            {
                con.Close();
            }
            return numEffected;
        }
        private SqlCommand CreateInsertBorrow(String spName, SqlConnection con, Borrow borrow) //הכנסת השאלה 
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure
            cmd.Parameters.AddWithValue("@startDate", borrow.StartDate);
            cmd.Parameters.AddWithValue("@endDate", borrow.EndDate);
            cmd.Parameters.AddWithValue("@startTime", borrow.StartTime);
            cmd.Parameters.AddWithValue("@endTime", borrow.EndTime);
            cmd.Parameters.AddWithValue("@parkingName", borrow.ParkingName);
            cmd.Parameters.AddWithValue("@email_user", borrow.Email);

            return cmd;
        }

        public List<object> AvailabilityCheck(DateTime desiredDate, DateTime startTime, DateTime endTime, string email, int idB) //הוצאת חניות רצוית לי בקשת המשתמש   

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            DateTime endDate = desiredDate;
            if (endTime<startTime)
            {
                endDate = desiredDate.AddDays(1);
            }
            cmd = CreateReadAvailabilityCheck("SPFindSuitableParkingSpaces", con, desiredDate, endDate, startTime, endTime,email,idB);  //שליפת חניות לפי בקשת המשתמש
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                List<object> availableParking = new List<object>();
                while (dataReader.Read())
                {
                    var parking = new
                    {
                        ParkingName = dataReader["parkingName"].ToString(),
                        StartDate = Convert.ToDateTime(dataReader["startDate"]),
                        EndDate = Convert.ToDateTime(dataReader["endDate"]),
                        //StartDate = DateTime.ParseExact(dataReader["startDate"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                        //EndDate = DateTime.ParseExact(dataReader["endDate"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                        StartTime = dataReader["startTime"].ToString(),
                        EndTime = dataReader["endTime"].ToString(),
                        FirstName = dataReader["firstName"].ToString(),
                        FamilyName = dataReader["familyName"].ToString(),
                        BorrowId = Convert.ToInt32(dataReader["id"]),
                        email = dataReader["email_user"].ToString(),
                        stars= Convert.ToInt32(dataReader["stars"]),


                    };
                    availableParking.Add(parking);
                }
                return availableParking;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        private SqlCommand CreateReadAvailabilityCheck(String spName, SqlConnection con, DateTime desiredDate, DateTime endDate, DateTime startTime, DateTime endTime, string email, int idB)//מחזיר שמות חניות מתאימות לפי בקשת המשתמש
        {

            SqlCommand cmd = new SqlCommand(); // create the command object----------

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@startDate", desiredDate);
            cmd.Parameters.AddWithValue("@endDate", endDate);
            cmd.Parameters.AddWithValue("@startTime", startTime);
            cmd.Parameters.AddWithValue("@endTime", endTime);
            cmd.Parameters.AddWithValue("@idBuilding", idB);
            return cmd;
        }

        public List<Borrow> GetUserBorrows(string mail)  //שליפת השאלות לפי מייל
        {

            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateUserDetailByEmail("SpReadUserBorrows", con, mail);  //שליפת חניות שלי בדף האישי
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                List<Borrow> UserBorrows = new List<Borrow>();
       
                while (dataReader.Read())
                {
                    Borrow B = new Borrow();
                    B.Id = Convert.ToInt32(dataReader["id"]);
                    B.EndDate = Convert.ToDateTime(dataReader["endDate"]);
                    B.StartDate = Convert.ToDateTime(dataReader["startDate"]);
                    //B.StartDate = DateTime.ParseExact(dataReader["startDate"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    //B.EndDate = DateTime.ParseExact(dataReader["endDate"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    B.StartTime = DateTime.Parse(dataReader["startTime"].ToString());
                    B.EndTime = DateTime.Parse(dataReader["endTime"].ToString());
                    B.ParkingName = dataReader["parkingName"].ToString();
                    B.Status = Convert.ToInt32(dataReader["status"]);
                    B.Email = dataReader["email_user"].ToString();
                    UserBorrows.Add(B);
                };
                return UserBorrows;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }//הוצאת החניות של המשתמש
        
        public int UpdateBorrows(Borrow borrow)//עדכון פרטי השאלה
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            //String cStr = BuildInsertCommand(flat);      // helper method to build the insert string
            cmd = CreateUpdateCommandSP("SpUpdateBorrow", con, borrow);             // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }
        private SqlCommand CreateUpdateCommandSP(String spName, SqlConnection con, Borrow B)//עדכון פרטי השאלה של המשתמש
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@id", B.Id);
            cmd.Parameters.AddWithValue("@startTime", B.StartTime);
            cmd.Parameters.AddWithValue("@endTime", B.EndTime);
            cmd.Parameters.AddWithValue("@email_user", B.Email);




            return cmd;
        }

        public int DeleteBorrows(int id, string mail)//מחיקת השאלה
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            //String cStr = BuildInsertCommand(flat);      // helper method to build the insert string
            cmd = CreateDeleteBorrowCommandSP("SpDeleteBorrow",con,id,mail);             // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }
        private SqlCommand CreateDeleteBorrowCommandSP(String spName, SqlConnection con,int id, string mail)//מחיקת השאלה
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@id",id);
            cmd.Parameters.AddWithValue("@email", mail);




            return cmd;
        }

        public int InsertRequest(Request Request) //הזנת בקשה לחניה לדף הבניין הכללי
        {
            SqlConnection con;
            SqlCommand cmd;
            int numEffected = 0;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }            
            cmd = CreateInsertRequests("InsertRequest", con, Request); //להכניס כמו שזה
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                int number = 0;
                while (dataReader.Read())
                {
                    number = Convert.ToInt32(dataReader["RequestID"]);
                };
                return number;
            }
            catch (Exception ex)
            {
                    // write to log
               throw (ex);
            }          
            if (con != null)
            {
                con.Close();
            }
            return numEffected;
        }
        private SqlCommand CreateInsertRequests(String spName, SqlConnection con, Request Request) //הזנת בקשה לחניה לדף הבניין הכללי
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure
            cmd.Parameters.AddWithValue("@startDate", Request.StartDate);
            cmd.Parameters.AddWithValue("@endDate", Request.EndDate);
            cmd.Parameters.AddWithValue("@startTime", Request.StartTime);
            cmd.Parameters.AddWithValue("@endTime", Request.EndTime);
            cmd.Parameters.AddWithValue("@email_user", Request.Email);
            cmd.Parameters.AddWithValue("@id", Request.Id);  //כדי שנוכל לשלוף את האידי שניתן לו
            return cmd;
        }

        public Borrow getBorrowD(int idBorrow) //שליפת השאלה לפי אי.די    
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = getByIdBorrow("SPgetBorrow", con, idBorrow);  //קראנו להשאלה לפי אי.די
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                Borrow borrow = new Borrow();
                TimeSpan startTime;  // SQLהגדרה של זה כי ככה הוא מוגדר ב
                TimeSpan endTime;
                while (dataReader.Read()) //במידה ומצא את ההשאלה 
                {
                    borrow.Id = Convert.ToInt32(dataReader["id"]);

                    borrow.StartDate = Convert.ToDateTime(dataReader["startDate"]);
                    borrow.EndDate = Convert.ToDateTime(dataReader["endDate"]);
                    //borrow.StartDate = (DateTime)dataReader["startDate"];
                    //borrow.EndDate = (DateTime)dataReader["endDate"];

                    startTime = dataReader.GetTimeSpan(dataReader.GetOrdinal("startTime")); ///שליפה
                    endTime = dataReader.GetTimeSpan(dataReader.GetOrdinal("endTime"));

                    borrow.StartTime = new DateTime() + startTime; //המרה לפורמט מתאים 
                    borrow.EndTime = new DateTime() + endTime; //כי ככה הזמן מוגדר בהשאלה

                    borrow.ParkingName = dataReader["parkingName"].ToString();
                    borrow.Status = Convert.ToInt32(dataReader["status"]);
                    borrow.Email = dataReader["email_user"].ToString();

                }
                return borrow;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        public List<Request> getSmartRequest(int BorrowId, int BuildingID) //שליפת בקשות מתאימות להשאלה הנוכחית לאותו הבניין    
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = GetByIdBorrowAndIdBuilding("SPgetRequestAccordingBorrow", con, BorrowId, BuildingID);
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                
                TimeSpan startTime;  // SQLהגדרה של זה כי ככה הוא מוגדר ב
                TimeSpan endTime;
                List<Request> RequestList = new List<Request>();

                while (dataReader.Read()) //כל הבקשות שמתאימות לפרטי ההשאלה 
                {
                    Request Request = new Request();

                    Request.Id = Convert.ToInt32(dataReader["id"]);

                    Request.StartDate = Convert.ToDateTime(dataReader["startDate"]);
                    Request.EndDate = Convert.ToDateTime(dataReader["endDate"]);

                    startTime = dataReader.GetTimeSpan(dataReader.GetOrdinal("startTime")); ///שליפה
                    endTime = dataReader.GetTimeSpan(dataReader.GetOrdinal("endTime"));

                    Request.StartTime = new DateTime() + startTime; //המרה לפורמט מתאים 
                    Request.EndTime = new DateTime() + endTime; //כי ככה הזמן מוגדר בהשאלה

                    Request.Status = Convert.ToInt32(dataReader["status"]);
                    Request.Email = dataReader["email_user"].ToString();
                    RequestList.Add(Request);
                }
                return RequestList;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        private SqlCommand GetByIdBorrowAndIdBuilding(String spName, SqlConnection con, int BorrowId, int BuildingID)  //שליפת השאלה
        {

            SqlCommand cmd = new SqlCommand(); // create the command object---------------------------------------

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure


            cmd.Parameters.AddWithValue("@BorrowId", BorrowId);
            cmd.Parameters.AddWithValue("@BuildingID", BuildingID);

            return cmd;
        }

        public int GetUserRate(string email_receiver)  //שליפת הדירוג של המשתמש לפי מייל
        {

            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateUserDetailByEmail("SPAvgRating", con, email_receiver);  //
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                int UserRating = 0;
                while (dataReader.Read())
                {
                    UserRating = Convert.ToInt32(dataReader["avg_rating"]);
                };

                return UserRating;
            }
            catch (SqlException ex)//תפיסת זריקה של sql
            {
                Console.WriteLine("Error reading Data", ex.Message);
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }//הוצאת הדירוג של המשתמש
        //לבדוק אם צריך פעמיים את הקריאה לsql כי כבר קיים כזה. 

        public int UpdateUserRate(string mail, int rate)//עדכון הדירוג של המשתמש
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            //String cStr = BuildInsertCommand(flat);      // helper method to build the insert string
            cmd = CreateUpdateRateCommandSP("SPUpdateRating", con, mail, rate);             // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }
        private SqlCommand CreateUpdateRateCommandSP(String spName, SqlConnection con, string email, int rate)//עדכון דירוג המשתמש
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Stars", rate);

            return cmd;
        }

        public List<object> GetArchive(string mail) //הוצאת מידע ארכיוני למשתמש   

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            cmd = CreateUserDetailByEmail("SPArchiveList", con, mail);  //שליפת חניות לפי בקשת המשתמש
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                List<object> ArchiveUser = new List<object>();
                while (dataReader.Read())
                {
                    var Archive = new
                    {
                        BorrowId = Convert.ToInt32(dataReader["id_Borrow"]),
                        RequestId = Convert.ToInt32(dataReader["id_Request"]),
                        BorrowStartDate = dataReader["borrow_SD"],
                        BorrowEndDate = dataReader["borrow_ED"],
                    //BorrowStartDate = DateTime.ParseExact(dataReader["borrow_SD"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                    //BorrowEndDate = DateTime.ParseExact(dataReader["borrow_ED"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                        BorrowStartTime = dataReader["borrow_ST"].ToString(),
                        BorrowEndTime = dataReader["borrow_ET"].ToString(),
                        BMail = dataReader["borrow_Email"].ToString(),
                        RequestStartDate = dataReader["request_SD"],
                        RequestEndDate = dataReader["request_ED"],
                        //RequestStartDate = DateTime.ParseExact(dataReader["request_SD"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                        //RequestEndDate = DateTime.ParseExact(dataReader["request_ED"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                        RequestStartTime = dataReader["request_ST"].ToString(),
                        RequestEndTime = dataReader["request_ET"].ToString(),
                        RMail = dataReader["request_Email"].ToString(),
                        BFirstName = dataReader["borrow_firstName"].ToString(),
                        BFamilyName = dataReader["borrow_familyName"].ToString(),
                        RFirstName = dataReader["request_firstName"].ToString(),
                        RFamilyName = dataReader["request_familyName"].ToString(),
                        parkingName=dataReader["parkingName"].ToString(),
                        RateOrNot = Convert.ToInt32(dataReader["has_rating"])

                    };
                    ArchiveUser.Add(Archive);
                }
                return ArchiveUser;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }

        public int InserRate(Rating R) // הכנסה של דירוג
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateInsertRateCommand("SPInsertRating", con,R);     // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        private SqlCommand CreateInsertRateCommand(String spName, SqlConnection con, Rating R) //הכנסת דירוג
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure
            cmd.Parameters.AddWithValue("@email_giver", R.Email_giver);
            cmd.Parameters.AddWithValue("@email_reciver",R.Email_reciver);
            cmd.Parameters.AddWithValue("@borrow", R.Id_Borrow);
            cmd.Parameters.AddWithValue("@request", R.Id_Request);
            cmd.Parameters.AddWithValue("@rate", R.Grade);
            cmd.Parameters.AddWithValue("@notes ", R.Notes);
            
            return cmd;
        }

        public List<object> GetMatch(int borrowId) //הוצאת רשימה של מאצים    

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateReadCanceledMatch("SPReadMatch", con, borrowId);  //שליפת מאצים לפי ההשאלה 
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                List<object> CanceledMatch = new List<object>();
                while (dataReader.Read())
                {
                    var unmatch = new
                    {
                        BorrowId = Convert.ToInt32(dataReader["id_Borrow"]),
                        RequestId = Convert.ToInt32(dataReader["id_Request"]),
                        status = Convert.ToInt32(dataReader["status"]),
                        BorrowStartDate = Convert.ToDateTime(dataReader["borrow_SD"]).Date,
                        BorrowEndDate = Convert.ToDateTime(dataReader["borrow_ED"]).Date,
                        BorrowStartTime = TimeSpan.Parse(dataReader["borrow_ST"].ToString()),
                        BorrowEndTime = TimeSpan.Parse(dataReader["borrow_ET"].ToString()),
                        BMail = dataReader["borrow_Email"].ToString(),
                        RequestStartDate = Convert.ToDateTime(dataReader["request_SD"]).Date,
                        RequestEndDate = Convert.ToDateTime(dataReader["request_ED"]).Date,
                        RequestStartTime = DateTime.Today + TimeSpan.Parse(dataReader["request_ST"].ToString()),
                        RequestEndTime = DateTime.Today + TimeSpan.Parse(dataReader["request_ET"].ToString()),
                        RMail = dataReader["request_Email"].ToString(),
                        BFirstName = dataReader["borrow_firstName"].ToString(),
                        BFamilyName = dataReader["borrow_familyName"].ToString(),
                        RFirstName = dataReader["request_firstName"].ToString(),
                        RFamilyName = dataReader["request_familyName"].ToString(),
                        RequestStars= Convert.ToInt32(dataReader["request_stars"])

                    };
                    CanceledMatch.Add(unmatch);
                }
                return CanceledMatch;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        private SqlCommand CreateReadCanceledMatch(String spName, SqlConnection con, int borrowId)//הוצאת רשימת מאצים שהעבורם ההשאלה בוטלה
        {

            SqlCommand cmd = new SqlCommand(); // create the command object----------

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@idBorrow", borrowId);

            return cmd;
        }

        public int DeleteMatch(int idborrow, int idrequest)//מחיקת מאץ
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            //String cStr = BuildInsertCommand(flat);      // helper method to build the insert string
            cmd = CreateMatch("SpDeleteMatch", con, idborrow, idrequest);             // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }
        
        public int UpdateRequestStatus(int idRequest, int status)//עידכון סטטוס בקשה 
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            //String cStr = BuildInsertCommand(flat);      // helper method to build the insert string
            cmd = CreateUpdateStatusCommandSP("SPUpdateStatuseRequest", con, idRequest, status);             // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }
        private SqlCommand CreateUpdateStatusCommandSP(String spName, SqlConnection con, int id, int status)//עדכון סטטוס בקשה
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@status", status);




            return cmd;
        }

        public int InsertToAsk_for(int idBorrow, int idRequest) // הכנסה לטבלת מאץ
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            if (idBorrow <= 0)//אם קיבל מספר שלילי
            {
                throw new ArgumentException("Invalid idBorrow parameter value. The value must be a positive non-zero integer.");
            }
            if (idRequest <= 0)//אם קיבל מספר שלילי
            {
                throw new ArgumentException("Invalid idRequest parameter value. The value must be a positive non-zero integer.");
            }

            cmd = CreateMatch("SPInsertMatch", con, idBorrow, idRequest);    // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }
        private SqlCommand CreateMatch(String spName, SqlConnection con, int idBorrow, int idRequest) //הכנסה לטבלת מאץ
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure
            cmd.Parameters.AddWithValue("@idBorrow", idBorrow);
            cmd.Parameters.AddWithValue("@idRequest", idRequest);
            return cmd;
        }

        public int UpdateAsk_forStatus(int idBorrow, int idRequest, int status)//עדכון סטטוס בטבלת מאץ 
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateUpdateAsk_forStatusCommandSP("SPUpdateAsk_forStatus", con, idBorrow, idRequest, status);             // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }
        private SqlCommand CreateUpdateAsk_forStatusCommandSP(String spName, SqlConnection con, int idBorrow, int idRequest, int status)//עדכון סטטוס בטבלת מאץ
        {

            SqlCommand cmd = new SqlCommand(); // create the command object

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@idBorrow", idBorrow);
            cmd.Parameters.AddWithValue("@idRequest", idRequest);
            cmd.Parameters.AddWithValue("@status", status);
            return cmd;
        }

        public object GetNameRequest(string mail, int idRequest) //שליפת פרטי בקשה עם שם המשתמש
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            cmd = CreateReadNameRequest("SPReadNameRequest", con, mail, idRequest);  //שליפת חניות לפי בקשת המשתמש
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                object RequestWithUserD = new object();
                while (dataReader.Read())
                {
                    RequestWithUserD= new
                    {
                        RequestId = Convert.ToInt32(dataReader["id"]),
                        RequestStartDate = Convert.ToDateTime(dataReader["startDate"]),
                        RequestEndDate = Convert.ToDateTime(dataReader["endDate"]),
                    //RequestStartDate = DateTime.ParseExact(dataReader["startDate"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).Date,
                    //RequestEndDate = DateTime.ParseExact(dataReader["endDate"].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).Date,
                        RequestStartTime = DateTime.Today + TimeSpan.Parse(dataReader["startTime"].ToString()),
                        RequestEndTime = DateTime.Today + TimeSpan.Parse(dataReader["endTime"].ToString()),
                        RequestStatus = Convert.ToInt32(dataReader["status"]),
                        RMail = dataReader["email_user"].ToString(),
                        UserFirstName = dataReader["firstName"].ToString(),
                        UserLastName = dataReader["familyName"].ToString(),
                        UserStars = Convert.ToInt32(dataReader["stars"])
                    };
                    
                }
                return RequestWithUserD;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }
        private SqlCommand CreateReadNameRequest(String spName, SqlConnection con,string mail, int idRequset)// קבלת הבקשה עם פרטי המשתמש
        {

            SqlCommand cmd = new SqlCommand(); // create the command object----------

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@mail", mail);
            cmd.Parameters.AddWithValue("@idRequest", idRequset);

            return cmd;
        }

        public int GetBuildingID(int idBorrow) //שליפת אידי של בניין לפי השאלה    
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = getByIdBorrow("SpReadBuildingIdByBorrow", con, idBorrow); 
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                int BuildingID=0;
                while (dataReader.Read()) //במידה ומצא את הבניין 
                {
                    BuildingID = Convert.ToInt32(dataReader["id"]);
                }
                return BuildingID;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }

        private SqlCommand getByIdBorrow(String spName, SqlConnection con, int id)  //שליפת השאלה או בקשה
        {

            SqlCommand cmd = new SqlCommand(); // create the command object---------------------------------------

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure


            cmd.Parameters.AddWithValue("@id", id);

            return cmd;
        }

        public object GetcheckMatch(int borrowId, int RequstId) //בדיקה האם קיים המאץ' הספציפי    

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateReadcheckMatch("SPcheckMatch", con, borrowId, RequstId);  //שליפת מאצים לפי ההשאלה 
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                int CheckedMatch = 0;
                while (dataReader.Read())
                {
                    CheckedMatch = 1;
                };
                return CheckedMatch;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        private SqlCommand CreateReadcheckMatch(String spName, SqlConnection con, int borrowId, int requestId)//הוצאת רשימת מאצים שהעבורם ההשאלה בוטלה
        {

            SqlCommand cmd = new SqlCommand(); // create the command object----------

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@idBorrow", borrowId);
            cmd.Parameters.AddWithValue("@idRequest", requestId);


            return cmd;
        }

        public List<object> GetApproveMatch(int borrowId) //הוצאת רשימה של מאצים כדי לבדוק האם ההשאלה מנוצלת עד הסוף    

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateReadCanceledMatch("SPReadApproveMatch", con, borrowId);  //שליפת מאצים לפי ההשאלה 
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                List<object> ApproveMatch = new List<object>();
                while (dataReader.Read())
                {
                    var unmatch = new
                    { /*Convert.ToDateTime(dataReader["request_SD"]);*/
                        RequestStartDate = Convert.ToDateTime(dataReader["request_SD"]).Add(TimeSpan.Parse(dataReader["request_ST"].ToString())),
                        RequestEndDate = Convert.ToDateTime(dataReader["request_ED"]).Add(TimeSpan.Parse(dataReader["request_ET"].ToString())),
                        RequestStartTime = DateTime.Today + TimeSpan.Parse(dataReader["request_ST"].ToString()),
                        RequestEndTime = DateTime.Today + TimeSpan.Parse(dataReader["request_ET"].ToString())
                    };
                    ApproveMatch.Add(unmatch);
                }
                return ApproveMatch;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }

        public int GetcheckParkingName(int idBuilding, string parkingName) //בדיקה האם קיים המאץ' הספציפי    

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreatecheckParkingName("SPCheckParkingNameExist", con, idBuilding, parkingName);  //שליפת מאצים לפי ההשאלה 
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                int CheckedParkingName = 0;
                while (dataReader.Read())
                {
                    CheckedParkingName = 1;
                };
                return CheckedParkingName;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }
        private SqlCommand CreatecheckParkingName(String spName, SqlConnection con, int idBuilding, string parkingName)//הוצאת רשימת מאצים שהעבורם ההשאלה בוטלה
        {

            SqlCommand cmd = new SqlCommand(); // create the command object----------

            cmd.Connection = con;              // assign the connection to the command object

            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be stored procedure

            cmd.Parameters.AddWithValue("@idBuilding", idBuilding);
            cmd.Parameters.AddWithValue("@parkingName", parkingName);


            return cmd;
        }
        public int DeleteUser(string mail) //בדיקה האם קיים המאץ' הספציפי    

        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateUserDetailByEmail("SPDeleteUser", con,mail);  //שליפת מאצים לפי ההשאלה 
            try
            { 
                int numEffected = cmd.ExecuteNonQuery();
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }

        public Request getRequest(int idRequest) //שליפת בקשה לפי אי.די    
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = getByIdBorrow("SPgetRequest", con, idRequest);  //קראנו להשאלה לפי אי.די
            try
            {
                SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                Request request = new Request();
                TimeSpan startTime;  // SQLהגדרה של זה כי ככה הוא מוגדר ב
                TimeSpan endTime;
                while (dataReader.Read()) //במידה ומצא את ההשאלה 
                {
                    request.Id = Convert.ToInt32(dataReader["id"]);

                    request.StartDate = (DateTime)dataReader["startDate"];
                    request.EndDate = (DateTime)dataReader["endDate"];

                    startTime = dataReader.GetTimeSpan(dataReader.GetOrdinal("startTime")); ///שליפה
                    endTime = dataReader.GetTimeSpan(dataReader.GetOrdinal("endTime"));

                    request.StartTime = new DateTime() + startTime; //המרה לפורמט מתאים 
                    request.EndTime = new DateTime() + endTime; //כי ככה הזמן מוגדר בהשאלה

                    request.Status = Convert.ToInt32(dataReader["status"]);
                    request.Email = dataReader["email_user"].ToString();

                }
                return request;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }

        }


        public int UpdateBorrows(int idBorrow, int status)//עדכון סטטוס של השאלה 
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = connect("myProjDB"); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            cmd = CreateUpdateStatusCommandSP("SPUpdateBorrowStatus", con, idBorrow, status);             // create the command
            try
            {
                int numEffected = cmd.ExecuteNonQuery(); // execute the command
                return numEffected;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null)
                {
                    // close the db connection
                    con.Close();
                }
            }
        }
       

    }


}



