$(document).ready(function () {
    renderUser();
});


function renderUser() {  //מרנדר את שם המשתמש והמטבעות

    const Details = sessionStorage.getItem('userLogin');

    // Parse the string to an object
    const User = JSON.parse(Details);

    // Retrieve the values from the object
    const firstName = User.firstName;
    const coins = User.coins;
    str = " <p id='p-User'>שלום, " + firstName + "  " + "<img id='coinsimage' src='images/Coins.png'/>" + coins + "</p>";
    document.getElementById("ShowUser").innerHTML = str;
}