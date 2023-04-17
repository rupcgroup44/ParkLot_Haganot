
$(document).ready(function () {
    if (
        location.hostname == "localhost" ||
        location.hostname == "127.0.0.1" ||
        location.hostname == ""
    ) {
        api = "https://localhost:7006/api/User";
    } else {
          api = "https://proj.ruppin.ac.il/cgroup44/prod/api/User";
    }

    $("#Log-Form").submit(function () { //כניסה למערכת
        UserLogin();
        return false;
    })
    $('#myModal').on('hidden.bs.modal', function () {
        if ($('#title').text() == 'התחברת בהצלחה!') {
            window.location = "PersonalArea.html";
        }
    });
})

function UserLogin() { //התחברות למערכת
    mail = $("#email").val();
    password = $("#pwd").val();
    buildingCode = $("#Building").val();
/*    api = "https://localhost:7006/api/User";*/
    ajaxCall("GET", api + "/" + mail, "", getLoginSCB, getLoginECB);
}

function getLoginSCB(obj) { //ההתחברות הצליחה
    console.log(obj);
    if (obj.email == mail && obj.password == password && obj.buildingCode == buildingCode) {
        MessageToUser('התחברת בהצלחה!', 'תועבר לדף הבית לאחר לחיצה על כפתור ה-X');
    }
    else if (obj.email == mail && obj.password == password && obj.buildingCode != buildingCode) {
        MessageToUser('התחברת נכשלה', 'קוד בניין שגוי,אנא נסה/י שנית');
    }
    else if (obj.email == mail && obj.password != password && obj.buildingCode == buildingCode) {
        MessageToUser('התחברת נכשלה', 'סיסמא שגויה, אנא נסה/י שנית');
    }
    else {
        MessageToUser('התחברת נכשלה', 'קוד בניין וסיסמא שגויים, אנא נסה/י שנית');
         }
    sessionStorage.setItem("userLogin", JSON.stringify(obj));  //הופכת אובייקט למחרוזת ומאחסנת
}

function getLoginECB(erorr) { //התחברות נכשלה
    MessageToUser('משתמש לא רשום', 'נא הירשם/י לאתר ולאחר מכן בצע התחברות ');
}

const hebrewToLatin = {
    "א": "a",
    "ב": "b",
    "ג": "g",
    "ד": "d",
    "ה": "h",
    "ו": "v",
    "ז": "z",
    "ח": "kh",
    "ט": "t",
    "י": "y",
    "כ": "k",
    "ל": "l",
    "מ": "m",
    "ם": "m",
    "נ": "n",
    "ס": "s",
    "ע": "a",
    "פ": "f",
    "צ": "ts",
    "ק": "q",
    "ר": "r",
    "ש": "sh",
    "ת": "t"
};

// פונקציה שמקבלת 3 פרמטרים- עיר רחוב ומספר בניין ומחזירה קוד בניין כללי

function createBuildingCode(city, street, buildingNum) {
    let latinCity = city.split("").map(c => hebrewToLatin[c] || c).join("");
    let latinStreet = street.split("").map(c => hebrewToLatin[c] || c).join("");
    return latinCity.slice(0, 2).toUpperCase() + latinStreet.slice(0, 2).toUpperCase() + ("111" + buildingNum).slice(-4);
}



