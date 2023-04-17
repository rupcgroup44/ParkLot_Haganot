function MessageToUser(title, description) {
    $('#title').text(title);
    $('#description').text(description);
    $('#myModal').modal('show');
}