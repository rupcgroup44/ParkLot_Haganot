
$(document).ready(function () {
    if (
        location.hostname == "localhost" ||
        location.hostname == "127.0.0.1" ||
        location.hostname == ""
    ) {
        api = "https://localhost:7006/api/Rating";
    } else {
        api = "https://proj.ruppin.ac.il/cgroup44/prod/api/Rating";
    }
     UserData = JSON.parse(sessionStorage.getItem("userLogin"));
     RatingData = JSON.parse(sessionStorage.getItem("Archive"));
     choosenData = JSON.parse(sessionStorage.getItem("ArchiveItem"));
     choosenRate = RatingData[choosenData];
    checkRateDetails();
    renderData(choosenRate);
    
    $('input[type="submit"]').click(function (e) {
        e.preventDefault(); // Prevent the default form submission

        // Get the selected rating and entered text
        UGrade = $('input[name="rating"]:checked').val();
        RateNotes = $('#RateNotes').val();
        SendRating();
    });
});


function checkRateDetails() {//פונקציית עזר להבין את תפקיד המשתמש 
    if (UserData.email == choosenRate.bMail) {//אם המשתמש שמחובר הוא זה שהשאיל את ההזמנה
        UserGetRate = choosenRate.rMail;//הוא יתן דירוג למי ששאל ממנו את החניה
    }
    else {
        UserGetRate = choosenRate.bMail//יתן דירוג למי שהוא השאיל ממנו
    }
}


function SendRating() {//שליחת פרטי הדירוג לשרת
    
    Rating = {
        email_giver: UserData.email,  //נותן דירוג
        email_reciver: UserGetRate,   //מקבל דירוג
        id_Borrow: choosenRate.borrowId,
        id_Request: choosenRate.requestId,
        Grade: UGrade,
        notes: RateNotes 
    }
    ajaxCall("POST", api, JSON.stringify(Rating), PostRateSCB, PostRateECB);

}
function PostRateSCB(data) {
    if (data == -1) {
        window.location = "ArchivesPage.html";
    }
    else {
        PostRateECB(data)
    }
}

function PostRateECB(err) {
    console.log("שגיאה בשליחת הדירוג, נא נסה/י שנית.")
}

function renderData(RatingData) {  //רינדור פרטי הדירוג
    BorrowStartTime = RatingData.borrowStartTime;
    StartTime = BorrowStartTime.substring(0, 5);
    BorrowEndTime = RatingData.borrowEndTime;
    EndTime = BorrowEndTime.substring(0, 5)
    RequestStartTime = RatingData.requestStartTime;
    RStartTime = RequestStartTime.substring(0, 5);
    RequestEndTime = RatingData.requestEndTime;
    REndTime = RequestEndTime.substring(0, 5);
    parkingName = RatingData.parkingName;
    
    var str = "";
    str += "<h1 id='ratedetails'>פרטי החניה:</h1>";
    str += "<div class='row'>";
    str += " <div class='col-6 ratingP'> <p> <strong>משאיל החניה:</strong> " + RatingData.bFirstName + " " + RatingData.bFamilyName + "</p></div>";
    str += " <div class='col-6 ratingP'> <p> <strong>מבקש החניה:</strong> " + RatingData.rFirstName + " " + RatingData.rFamilyName + "</p></div>";
    str += "</div>";
    str += "<div class='row'>";
    str += " <div class='col-6 ratingP'> <p> <strong>תאריך:</strong>  " + covertDate(RatingData.borrowStartDate) + "</p></div>";
    str += " <div class='col-6 ratingP'> <p> <strong>שם חניה:</strong> " + parkingName + "</p></div>";
    str += "</div>";
    str += "<div class='row'>";
    str += " <div class='col-6 ratingP'> <p> <strong>שעת התחלת ההשאלה:</strong> " + StartTime + "</p></div>";
    str += " <div class='col-6 ratingP'> <p> <strong>שעת סיום ההשאלה:</strong> " + EndTime + "</p></div>";
    str += "</div>";
    str += "<div class='row'>";
    str += " <div class='col-6 ratingP'> <p> <strong>שעת התחלת הבקשה:</strong> " + RStartTime + "</p></div>";
    str += " <div class='col-6 ratingP'> <p> <strong>שעת סיום הבקשה:</strong> " + REndTime + "</p></div>";
    str += "</div>";
    document.getElementById("R1").innerHTML = str; 
};

function covertDate(date) {
    const isoString = date;
    const dateObj = new Date(isoString);
    const day = dateObj.getDate().toString().padStart(2, "0");
    const month = (dateObj.getMonth() + 1).toString().padStart(2, "0");
    const year = dateObj.getFullYear();
    return dateString = `${day}-${month}-${year}`; // output: "11-03-2022"
}

function timeConversion(timeStr) {  //המרת הזמן שהמשתמש מכניס לפורמט מתאים לשרת

    let timeParts = timeStr.split(":");  // :מפצלת את השעה לפי 
    let dateObj = new Date();            //יוצרת תאריך
    dateObj.setHours(parseInt(timeParts[0]));    //לפורמת תאריך מוסיפה שעות שהמשתמש הזין
    dateObj.setMinutes(parseInt(timeParts[1]));  //לפורמת תאריך מוסיפה דקות שהמשתמש הזין
    dateObj.setSeconds(0);
    dateObj.setMilliseconds(0);

    // add two hours to the date object
    dateObj.setHours(dateObj.getHours() + 2);

    let formattedTime = dateObj.toISOString();  //2023-03-05T08:00:00Z
    return formattedTime;
}

