$(function () {
    document.getElementById('import-button').addEventListener('click', function () {
        var formId = this.getAttribute('data-form-id'); // Get the form id from data attribute
        document.getElementById(formId).submit(); // Submit the corresponding form
    });
});