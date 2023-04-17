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

    $("#RegisterForm").submit(function () { //הרשמה
        UserRegister();
        return false;
    })

    $("#parkingNumber").on("change", function () { //מוסיפה לטופס שם חניה בהתאם למספר החניות שהמשתמש הזין
        let numParkingSpaces = $(this).val(); //קבלת מספר החניות שיש למשתמש
        $("#parking-form-group").html(""); // clear the existing elements
        for (let i = 1; i <= numParkingSpaces && i <= 4; i++) {
            $("#parking-form-group").append(`
            <div>
                <label for="park${i}">שם חניה ${i}<span class="required-star">*</span></label>
                <input type="text" class="form-control" placeholder="A2${i}" id="park${i}"  required>
            </div>
            `);
        }
    });
    initAutocomplete();
        $('#myModal').on('hidden.bs.modal', function () { //הודעה קופצת
            if ($('#title').text() == 'נרשמת בהצלחה') {
                window.location = "loginParkingPage.html";
            }
        });
    });

function initAutocomplete() {
    var input = document.getElementById('address');
    var autocomplete = new google.maps.places.Autocomplete(input);

    autocomplete.addListener('place_changed', function () {
        var place = autocomplete.getPlace();

        // Check if the input corresponds to a valid address
        if (!place.geometry) {
            input.value = '';
            $('#address-validation-msg').text('Please enter a valid address.');
        } else {
            $('#address-validation-msg').text('');
        }
    });
}
    function initAutocomplete() {
        const center = { lat: 32.0853, lng: 34.7818 };

        const defaultBounds = {
            north: 33.349197,
            south: 32.996810,
            east: 35.132454,
            west: 34.782111
        };
        const input = document.getElementById("address");
        const options = {
            bounds: defaultBounds,
            componentRestrictions: { country: "il" },
            fields: ["address_components", "formatted_address", "geometry", "icon", "name"],
            strictBounds: false,
            types: ["establishment", "geocode"]
        };
        const autocomplete = new google.maps.places.Autocomplete(input, options);
    }

    function UserRegister() { //הרשמת משתמש

        email = $("#email").val();  //קבלת ערכים
        firstName = $("#firstName").val();
        familyName = $("#familyName").val();
        phone1 = $("#phone1").val();
        phone2 = $("#phone2").val();
        address = $("#address").val(); //כתובת מלאה רחוב+מספר+עיר

        let parts = address.split(", "); // פיצול הכתובת לפי פסיק
        let streetAndBuilding = parts[0].split(" "); //פיצול הרחוב ומספר בניין לפי רווח
        buildingNumber = streetAndBuilding.pop();  //פופ לוקח את החלק האחרון של המערך ומוריד אותו מהמערך
        street = streetAndBuilding.join(" ");  //החלקים שנותרו מן המערך שזה שם הרחוב נחבר עם רווח
        city = parts[1];

        password = $("#pwd").val();
        buildingCode = createBuildingCode(city, street, buildingNumber);

        User = { //יצירת אובייקט
            Email: email,
            FirstName: firstName,
            FamilyName: familyName,
            City: city,
            Street: street,
            Password: password,
            Coins: 10,
            IdBuilding: 0,
            BuildingCode: buildingCode,
            BuildingNumber: buildingNumber,
        }

        console.log(User);
        ajaxCall("POST", api + "?Codebuilding=" + buildingCode, JSON.stringify(User), postRegSCB, postRegECB); // יצירת משתמש ובניין-הרשמה משתמש

    }


    const hebrewToLatin = { //מילון
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

    function postRegSCB(data) { //במידה והצליח רישום המשתמש
        if (data == 1) {        //נכניס את מספרי החניות
            NumberofParking = $("#parkingNumber").val();
            objParking = {  //יצירת אובייקט כללי לחניות
                Parking1: null,
                Parking2: null,
                Parking3: null,
                Parking4: null,
                BuildingCode: buildingCode,
                Email: email
            }
            for (var i = 1; i <= NumberofParking; i++) {  // יצירת טבלה parking_spot
                objParking["Parking" + i] = $("#park" + i).val(); //אתחול שדות האובייקט
            }
            if (
                location.hostname == "localhost" ||
                location.hostname == "127.0.0.1" ||
                location.hostname == ""
            ) {
                var api = "https://localhost:7006/Parking";
            } else {
                var api = "https://proj.ruppin.ac.il/cgroup44/prod/Parking";
            }
            ajaxCall("POST", api, JSON.stringify(objParking), postParSCB, postParECB);
            console.log(data);
        }
        else postRegECB(data);
    }

    function postRegECB(erorr) { // ההרשמה של המשתמש נכשלה 
        MessageToUser('נכשל', 'ההרשמה נכשלה, אנא וודא אם הינך רשום כבר למערכת');
    }

function postParSCB(data) { //במידה והצליח להזין את מספרי החניות
        if (data >= 1) {        //נזין את הטלפונים של המשתמש
            o = {               //אובייקט כללי
                Phone1: phone1,
                Phone2: phone2,
                Email: email
            }
            ajaxCall("POST", api +"/Phone", JSON.stringify(o), postPhoneSCB, postPhoneECB);
        }
        else postParECB(data);
}

function postParECB(erorr) { // לא מצליח להכניס חניות לבסיס נתונים 
        MessageToUser('נכשל', 'חניות קיימות במערכת,נא הירשם/י שנית');
}

    function postPhoneSCB(data) { //הצליח להזין את הטלפונים
        if (data == 1 || data == 2) {
            if (
                location.hostname == "localhost" ||
                location.hostname == "127.0.0.1" ||
                location.hostname == ""
            ) {
                var api = "https://localhost:7006/api/Email";
            } else {
                var api = "https://proj.ruppin.ac.il/cgroup44/prod/api/Email";
            }
            Email = {
                EmailAddress: email,
                BuildingCode: buildingCode
            }
            ajaxCall("POST", api, JSON.stringify(Email), postEmailSCB, postEmailECB);
        }
        else postPhoneECB(data);
    }

    function postPhoneECB(erorr) { // לא מצליח להכניס טלפונים לבסיס נתונים 
        MessageToUser('נכשל', 'לא מצליח להכניס את הטלפונים');
    }

    function postEmailSCB(data) {
        if (data == 1) {
            MessageToUser('נרשמת בהצלחה', 'נרשמת בהצלחה לאחר לחיצה על כפתור ה-X תועבר לדף הכניסה');
        }
        else postEmailECB();
    }

    function postEmailECB(erorr) { // לא מצליח להכניס את המשתמש 
        MessageToUser('נכשל', 'ההרשמה נכשלה')
    };

