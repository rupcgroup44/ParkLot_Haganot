
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
    GetArchive();
    

});

function GetArchive() {//הוצאת ארכיון של המשתמש
    userData = JSON.parse(sessionStorage.getItem("userLogin"));
    email = userData.email;
    idBiluding = userData.idBuilding;
    let url = api + "?mail=" + email;
    ajaxCall("GET", url, "",GetArchiveSCB, GetArchiveECB);
}

function GetArchiveSCB(data) {
    console.log(data);
    if (data.length >= 1) {
        sessionStorage.setItem("Archive", JSON.stringify(data));  //שמירת פרטים כדי להעביר  לדף הבא
        renderArchive(data);
    } else {
        GetArchiveECB(data);
    }
}

function GetArchiveECB(erorr) {
    console.log(erorr);
    str = "<div class='gtco-container justify-content-center frame BorrowShow2'>"
    str += "<h1>לא נמצא מידע בארכיון</h1 >";
    str += "</div >";
    document.getElementById("Archive").innerHTML = str;
}


function renderArchive(data) {  //מרנדר את ההשאלות המתאימות
    var str = "";
    
    for (let i = 0; i < data.length; i++) { 
        BorrowStartDate = data[i].borrowStartDate;
         RequestStartTime = data[i].requestStartTime;
        StartTime = RequestStartTime.substring(0, 5);
         RequestEndTime = data[i].requestEndTime;
        endTime = RequestEndTime.substring(0, 5);
        str += "<div class='col-xs-4 gtco-container justify-content-center frame BorrowShowA dir_col' id=" +[i] + ">";
        str += " <p> <strong>משאיל החניה:</strong> " + data[i].bFirstName + " " + data[i].bFamilyName + "</p>";
        str += " <p> <strong>מבקש החניה:</strong> " + data[i].rFirstName + " " + data[i].rFamilyName + "</p>";
        str += " <p> <strong>תאריך התחלה:</strong>  " + covertDate(data[i].borrowStartDate) + "</p>";
        str += " <p> <strong>שעת התחלה:</strong> " + StartTime + "</p>";
        str += " <p> <strong>שעת סיום:</strong> " + endTime + "</p>";      
        str += "<input type='submit' id='btn-" + [i] + "'value='דירוג' onclick='Rate(" + [i] + ")' class='btn btn-primary btn-middle'/>";
        str += "</div>";     
    }
    document.getElementById("Archive").innerHTML = str;
    SetDesign(data);
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

function SetDesign(ArchiveData) {//אם כבר דירג את המאץ' העיצוב ישתנה ולא יהיה כפתור
    for (var i = 0; i < ArchiveData.length; i++) {
        if (ArchiveData[i].rateOrNot==1) {
        document.getElementById([i]).style.backgroundColor = "lightgray";
        document.getElementById("btn-"+[i]).style.display = "none";
        }   
    }
}

function Rate(item) {//מעבר לדף דירוג ושמירת הלחיצה של הדירוג למשתנה
     sessionStorage.setItem("ArchiveItem", JSON.stringify(item));
    window.location = "RateParkingPage.html";
}







