$(document).ready(function () {
    if (
        location.hostname == "localhost" ||
        location.hostname == "127.0.0.1" ||
        location.hostname == ""
    ) {
        api = "https://localhost:7006/api/Borrow";
    } else {
        api = "https://proj.ruppin.ac.il/cgroup44/prod/api/Borrow";
    }
    $('input[name="date-type"]').on('change', function () {//תפיסה של הבחירה אם טווח או בודד
        selectedOption = $('input[name="date-type"]:checked').val();
       
    });


    $("#insert_parking_form").submit(function () { //הזנת חניה
        checkChoosen()//בדיקה אם יום בודד או טווח ושליחת ההשאלה לשרת
        return false;
    });


    DisablePassSingleDates(); //הגבלת תאריכים בפקדים
    GetParkingName(); //שולף את שמות החניה של המשתמש

    userData = JSON.parse(sessionStorage.getItem("userLogin")); //דרך שניה
    email = userData.email;
    
    $("#end-time").on('change', checkCode);// אחרי שבוחר שעת סיום תעשה ולידציה שיש חצי שעה רווח
        
    $("#start-time").on('change', function() {
        DisableTimes();   // אם מזינים השאלה להיום הגבלת השעות שעברו 
    });

    $("#start-date").on('change', function() {  //במידה והמשתמש שינה יום התחלה תעשה בדיקה נוספת לשעות
        DisablePassSingleDates(); //תופס את הימים מחדש
        MinEndDate();
        DisableTimes();   // אם מזינים השאלה להיום הגבלת השעות שעברו
    });
   
   
    $('#myModal').on('hidden.bs.modal', function () { //הודעה קופצת
        
    });
   
});


function checkCode() {//פונקציית ולידציה לבדיקה של חצי שעה על טווח 
    const startTime = document.getElementById('start-time').value;
    const endTime = document.getElementById('end-time').value;

    if (startTime && endTime) {
        const time1 = new Date(`2000-01-01T${startTime}:00`);
        const time2 = new Date(`2000-01-01T${endTime}:00`);
        const diff = Math.abs(time1.getTime() - time2.getTime()) / (1000 * 60);//ההפרש בין השעות בערך מוחלט

        if (diff < 30) { //אם ההפרש קטן מחצי שעה
            document.getElementById('end-time').setCustomValidity('יש להזין לפחות חצי שעה לאחר שעת ההתחלה');
        } else {
            document.getElementById('end-time').setCustomValidity('');
        }
    } else {
        document.getElementById('end-time').setCustomValidity('');
    }    
}

function GetParkingName() { //הוצאת חניות לפי מייל של המשתמש
    
    let userData = JSON.parse(sessionStorage.getItem("userLogin")); //דרך שניה
    let email = userData.email;
    ajaxCall("GET", api + "/parking/" +email, "", getParkingSCB, getParkingECB);
}

function getParkingSCB(data) {//הצלחה בהוצאת החניות
    console.log(data);
    RenderParking(data)
}

function getParkingECB(err) {//כישלון בהוצאת החניות
    alert("בעיה בשליפת שמות החניות של המשתמש");
    console.log(err);
}

function RenderParking(data) {//מרנדר את שמות החניות של המשתמש לטופס
    str = "<select name='parking' id='parkingnames' class='form-group form-control hania'>";

    if (Array.isArray(data)) {
        for (let i = 0; i < data.length; i++) {
            str += "<option value='" + data[i] + "'>" + data[i] + "</option>";
        }
    } else {
        str += "<option value='" + data + "'>" + data + "</option>";
    }
    str += "</select>";
    document.getElementById("ph1").innerHTML = str;
}

function checkChoosen() { //כותב האם המשתמש בחר טווח או יום בודד ושולח נתונים בהתאם

    const parkingN = document.getElementById("parkingnames").value;  //מכניס את השם עצמו למשתה

    const sT = $("#start-time").val();  //שעת התחלה
    const startT = timeConversion(sT);

    const eT = $("#end-time").val();    //קליטת שעת סיום מהמשתמש         
    const endT = timeConversion(eT)

    if (selectedOption == "range")  //אם הזין טווח
    {
        //תפיסה של ערכי הטווח
        const SD = new Date($("#start-date").val());//קליטת תאריך בפורמט מסוים
        const startD = SD.toISOString();            // Output: 2022-03-01T00:00:00.000Z
        const ED = new Date($("#end-date").val())   //קליטת תאריך בפורמט מסוים
        const endD = ED.toISOString();             // Output: 2022-03-01T00:00:00.000Z

        Borrow = {
            startDate: startD,
            endDate: endD,
            startTime: startT,
            endTime: endT,
            parkingName: parkingN,
            email: email
        }
        ajaxCall("POST", api, JSON.stringify(Borrow), postBorrowSCB, postBorrowECB); // הכנסת חניה פנויה לבסיס נתונים
    }
    else {      //אם הזין יום בודד

        const ODay = new Date($("#free-day").val());
        const oneDay = ODay.toISOString();  // Output: 2022-03-01T00:00:00.000Z
       
        if (startT > endT) {  //במידה והזין אחרי השעה 12 בלילה

            const newDate = new Date(ODay); //מגדירה תאריך מחדש
            newDate.setDate(newDate.getDate() + 1);//מוסיפה יום אחד

            Borrow = {
                startDate: oneDay,
                endDate: newDate.toISOString(),   //תאריך הסיום הוא יום אחד יותר כי זה שעות של לפני בוקר של היום הבא
                startTime: startT,
                endTime: endT ,
                parkingName: parkingN,
                email: email
            }
        }
        else {
            Borrow = {
                        startDate: oneDay,
                        endDate: oneDay,   //תאריך סיום והתחלה אותו הדבר זה יום בודד
                        startTime: startT,
                        endTime: endT,
                        parkingName: parkingN,
                        email: email
            }
        }         
        ajaxCall("POST", api, JSON.stringify(Borrow), postBorrowSCB, postBorrowECB); // הכנסת חניה פנויה לבסיס נתונים
    }
}

function timeConversion(time) {  //המרת הזמן שהמשתמש מכניס לפורמט מתאים לשרת

    // Create a new Date object with the given time
    var dateObj = new Date('1970-01-01T' + time + '+02:00');

    // Add two hours to the date
    dateObj.setHours(dateObj.getHours() + 2);

    // Convert the date to the desired format and remove the trailing 'Z'
    var formattedDate = dateObj.toISOString().replace("Z", "");

    // Return the formatted date
    return formattedDate;
}

function postBorrowSCB(data) {
    MessageToUser('הצלחה','הזנת השאלה הוכנסה בהצלחה');
}

function postBorrowECB(erorr) {
    MessageToUser('נכשל', 'כבר קיימת השאלה לחניה זו');
}

//הצגת פקדים של תאריכים לפי בחירת המשתמש

function showSingle() {//מראה יום אחד לבחירה של תאריכים
    document.getElementById("single-day").style.display = "block";
    document.getElementById("date-range").style.display = "none"; 
};

function showRange() {// מציג 2 תאריכים לבחירה
    document.getElementById("single-day").style.display = "none";
    document.getElementById("date-range").style.display = "block";  
};

function DisablePassSingleDates() {//הגבלת תאריכים בפקדים
    // קביעת תאריך מינימלי
    today = new Date().toISOString().split('T')[0];// התאריך של היום
    selectDateS = document.getElementById('free-day');//  תאריך שהמשתמש בחר ביום בודד
    selectDateS.setAttribute('min', today);// הגבלת התאריך שלא יהיה ניתן להוסיף תאריך שעבר
    selectDateR1 = document.getElementById('start-date');//תאריך התחלה בטווח
    selectDateR2 = document.getElementById('end-date');//תאריך סיום בטווח
    selectDateR1.setAttribute('min', today);
    selectDateR2.setAttribute('min', today);
};

function MinEndDate()//משאירה מרווח של יום לפחות בהזנת טווח
{
    var StartdateValue = new Date(selectDateR1.value);
    const min = new Date(StartdateValue.getTime() + 24 * 60 * 60 * 1000).toISOString().split('T')[0];;
    selectDateR2.setAttribute('min', min);
}


function DisableTimes() {// אם מזינים השאלה להיום הגבלת השעות שעברו 
    var SdateValue = selectDateS.value;
    var RdateValue = selectDateR1.value;
    var check;
    if (selectedOption == "range")//אם הזין טווח
    {
        check = RdateValue;
    } else {
        check = SdateValue;
    }
    if (today == check) {
        var now = new Date();
        var currentTime = now.getHours() + ':' + now.getMinutes();
        startTimeInput = document.getElementById('start-time');//תפיסת שעת התחלה
        startTimeInput.setAttribute('min', currentTime);//מינימום שעה של עכשיו
    } else {
        startTimeInput.setAttribute('min', '');//מאפסת את המינימום
    }
};
  
    
        
