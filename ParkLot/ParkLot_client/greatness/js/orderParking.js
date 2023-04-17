
$(document).ready(function () {
    if (
        location.hostname == "localhost" ||
        location.hostname == "127.0.0.1" ||
        location.hostname == ""
    ) {
        api = "https://localhost:7006/api/Request";
    } else {
        api = "https://proj.ruppin.ac.il/cgroup44/prod/api/Request";
    }

    $("#order_parking_form").submit(function () { 
        checkBorrows();
        document.getElementById("res").style.display = "block";
        return false;
    });

    DisablePassSingleDates();

    $('#myModal').on('hidden.bs.modal', function () { //הודעה קופצת

    });

});

function checkBorrows() {// שליפת חניות מתאימות
    //קודם תפיסת נתונים
    orderD = new Date($("#order-parking").val());//קליטת תאריך בפורמט מסוים
    orderDate = orderD.toISOString();            // Output: 2022-03-01T00:00:00.000Z
    OrdersTD = $("#start-time-order").val();
    OrderstartTD = timeConversion(OrdersTD);
    OrdedeTD = $("#end-time-order").val();
    OrderendTD = timeConversion(OrdedeTD);
    userData = JSON.parse(sessionStorage.getItem("userLogin"));
    email = userData.email;
    idBiluding = userData.idBuilding;
    if (
        location.hostname == "localhost" ||
        location.hostname == "127.0.0.1" ||
        location.hostname == ""
    ) {
        var api = "https://localhost:7006/DesirableBorrow/OrderDate/";
    } else {
        var api = "https://proj.ruppin.ac.il/cgroup44/prod/DesirableBorrow/OrderDate/";
    }
    //לאחר מכן שליפת חניות מתאימות בהתאם
    ajaxCall("GET", api + orderDate + "/startTime/" + OrderstartTD + "/endTime/" + OrderendTD + "/email/" + email + "/idB/" + idBiluding, "", postOrderSCB, postOrderECB);

}

function postOrderSCB(data) {
    console.log(data);
    if (data.length >= 1) {
        AnotherOption();// הצגת אופציה להכנס לדף הבקשות הכללי
        renderBorrows(data);  //הצגת אופציה להזמין מהשאלות קיימות
    } else {
        postOrderECB(data);
    }  
}

function AnotherOption() {  // הצגת אופציה להכנס לדף הבקשות הכללי
    var str = "";
    str += "<div>";
    str += " <p> <strong>במידה ולא נמצאה השאלה המתאימה לך תוכל להכנס לדף הבקשות הכללי של הבניין</strong></p>";
    str += "<input type='button' id='yes' value='כן, תכניס אותי' onclick='Income()' class='btn btn-primary btn-middle'/>";
    str += "</div>"
    document.getElementById("AnotherOption").innerHTML = str;
}

function postOrderECB(erorr) {  //במידה ולא נמצאו התאמות לבקשה, שואלים אם רוצה להכנס לדף הבקשות הכללי
    console.log(erorr);
    str = "<div class='gtco-container justify-content-center frame BorrowShow2'>"
    str += "<h1>לא נמצאו התאמות לבקשתך</h1 >";
    str += "<p class='row_center'> האם תרצה להיכנס לרשימת הבקשות של הבניין ?</p>";
    str +="<div class='row row_center'>"
    str += "<input id='yesInsert' type='button' value='כן, תכניס אותי' class='btn btn-primary btn-middle2 col-6'/>";
    str += "</div ></div >";
    document.getElementById("res").innerHTML = str;
    /* במידה ולחץ כן*/
    $('#yesInsert').click(Income);
}

function Income(idB) {// הכנסה של הבקשה לבסיס הנתונים לא משנה אם זה בשביל הזמנה או הבקשה הכללית של הבניין
    if (OrderstartTD > OrderendTD) { //במידה והזין משמרת לילה

        const newDate = new Date(orderDate); //מגדירה תאריך מחדש
        newDate.setDate(newDate.getDate() + 1);//מוסיפה יום אחד

        Request = {
            id: 1,
            startDate: orderDate,
            endDate: newDate.toISOString(),
            startTime: OrderstartTD,
            endTime: OrderendTD,
            email: email
        }

    } else {
        Request = {
            id:1,
            startDate: orderDate,
            endDate: orderDate,
            startTime: OrderstartTD,
            endTime: OrderendTD,
            email: email
        }
    }
    IdBorrowNow = idB; //משתנה עזר הפכנו לגלובלי
    ajaxCall("POST", api, JSON.stringify(Request), postRequestSCB, postRequestECB); // הכנסת בקשה לבסיס נתונים
}

function postRequestSCB(data) { //הצלחה בהכנסת הבקשה למסד הנתונים
    IdRequestnNow = data; //המספר בקשה של הבקשה שהכנסו הרגע
    if (IdBorrowNow>=1) {
        SendRequest(IdBorrowNow);  //הכנסה לטבלת מאץ
    }
    else MessageToUser('הצלחה', 'הבקשה נכנסה בהצלחה!');
}

function postRequestECB(erorr) {
    MessageToUser('נכשל', 'הבקשה לא נכנסה חלה בעיה!');
}

function renderBorrows(data) {  //מרנדר את ההשאלות המתאימות
    var str = "";
    for (let i = 0; i < data.length; i++) {
        dateStart = covertDate(data[i].startDate);
        dateEnd = covertDate(data[i].endDate);
        const timeString = data[i].startTime;
        const timeStringStart = timeString.substring(0, 5); // output: "10:30"
        const timeString2 = data[i].endTime
        const timeStringEnd = timeString2.substring(0, 5); // output: "10:30"
        str += "<div class='col-xs-4 gtco-container justify-content-center frame BorrowShow dir_col'>";
        str += " <p> <strong>שם חניה:</strong> " + data[i].parkingName + "</p>";
        str += " <p> <strong>תאריך התחלה:</strong>  " + dateStart + "</p>";
        str += " <p> <strong>תאריך סיום:</strong> " + dateEnd + "</p>";
        str += " <p> <strong>שעת התחלה:</strong> " + timeStringStart + "</p>";
        str += " <p> <strong>שעת סיום:</strong> " + timeStringEnd + "</p>";
        str += " <p> <strong>שם המשאיל/ה:</strong> " + data[i].firstName + " " + data[i].familyName + "</p>";
        str += " <p id=BorrowStars> <strong>דירוג:</strong>";
        for (var j = 0; j < data[i].stars; j++) {
            str += "<img src='images/Rating.png'/>"
        } 
       str+="</p>";
        str += "<input type='button' id='" + data[i].borrowId + "' name='" + data[i].email + "' value='הזמנה' onclick='Income(this.id)' class='btn btn-primary btn-middle'/>";
        str += "</div>";
    }
    document.getElementById("res").innerHTML = str;
}

function SendRequest(borrowID) { //הכנסת הבקשה לטבלת מאץ

    if (
        location.hostname == "localhost" ||
        location.hostname == "127.0.0.1" ||
        location.hostname == ""
    ) {
        var api = "https://localhost:7006/Match";
    } else {
        var api = "https://proj.ruppin.ac.il/cgroup44/prod/Match";
    }
    obj = {
        IdBorrow: borrowID,    //תפסנו
        IdRequest: IdRequestnNow  //גלובלי קבלנו על ידי פונקצית הצלחה של הכנסת בקשה 
    }
    ajaxCall("POST", api, JSON.stringify(obj), postMatchSCB, postMatchECB); // הכנסת מאץ לטבלה
}

function postMatchSCB(data) { //במידה והצליח להכניס לטבלאות
    if (data >= 1) {
        const submitBtn = document.getElementById(IdBorrowNow);
        const email = submitBtn.getAttribute("name");
        if (
            location.hostname == "localhost" ||
            location.hostname == "127.0.0.1" ||
            location.hostname == ""
        ) {
            var api = "https://localhost:7006/request";
        } else {
            var api = "https://proj.ruppin.ac.il/cgroup44/prod/request";
        }
        objForMail = {
            IdBorrow: IdBorrowNow,    //תפסנו
            Email: email  //גלובלי קבלנו על ידי פונקצית הצלחה של הכנסת בקשה
        }
        IdBorrowNow;
        ajaxCall("POST", api, JSON.stringify(objForMail), postMAILSCB, postMAILECB); // שליחת מייל למשאיל
    } else postMatchECB; 
}

function postMatchECB(erro) { //במידה ולא הצליח להכניס לטבלאות
    MessageToUser('נכשל', 'יש בעיה בשליחת הבקשה');
}

function postMAILSCB(data) { //במידה והצליח לשלוח מייל
    MessageToUser('הצלחה', 'נשלחה בקשה למשאיל/ה, ברגע שהבקשה תאושר תקבל מייל על כך');
}

function postMAILECB(erro) {  //במידה ולא הצליח לשלוח מייל
    MessageToUser('נכשל', 'יש בעיה בשליחת הבקשה');
}

function covertDate(date) {
    const isoString = date;
    const dateObj = new Date(isoString);
    const day = dateObj.getDate().toString().padStart(2, "0");
    const month = (dateObj.getMonth() + 1).toString().padStart(2, "0");
    const year = dateObj.getFullYear();
    return dateString = `${day}-${month}-${year}`; // output: "11-03-2022"
}

function timeConversion(timeStr) {  //המרת הזמן שהמשתמש מכניס לפורמט מתאים לשרת

    // Create a new Date object with the current date
    const currentDate = new Date();

    // Set the hours and minutes of the Date object using the user's input
    const [hours, minutes] = timeStr.split(":");
    currentDate.setHours(hours);
    currentDate.setMinutes(minutes);
    currentDate.setSeconds(0);
    currentDate.setMilliseconds(0);

    // Add three hours to the Date object
    currentDate.setTime(currentDate.getTime() + (3 * 60 * 60 * 1000));
    // Format the Date object as a string in the desired format
    const formattedDate = currentDate.toISOString();

    console.log(formattedDate); // Output: "2023-04-16T08:00:00.000Z"
    return formattedDate;
}

function DisablePassSingleDates() {//הגבלת תאריכים בפקדים
    // קביעת תאריך מינימלי
    today = new Date().toISOString().split('T')[0];// התאריך של היום
    selectDateS = document.getElementById('order-parking');//  תאריך שהמשתמש בחר 
    selectDateS.setAttribute('min', today);// הגבלת התאריך שלא יהיה ניתן להוסיף תאריך שעבר
    $("#order-parking").on('change', function () {
        DisableTimes();
    });
};

function DisableTimes() {// אם מזינים השאלה להיום הגבלת השעות שעברו 
    var dateValue = selectDateS.value;
    console.log('Selected date:', dateValue);
    if (today == dateValue) {//אם המשתמש רוצה להזין השאלה להיום נגביל את השעות
        var now = new Date();
        var currentTime = now.getHours() + ':' + now.getMinutes();
        startTimeInput = document.getElementById('start-time-order');//תפיסת שעת התחלה
        endTimeInput = document.getElementById('end-time-order');//תפיסת שעת סיום
        startTimeInput.setAttribute('min', currentTime);//מינימום שעה של עכשיו
    }
};
