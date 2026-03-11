$(document).ready(function () {

    $("#signupForm").submit(function (e) {
        e.preventDefault();
        var password = $("#password").val();
        var confirmPassword = $("#Confirmpassword").val();

        if (password !== confirmPassword) {
            e.preventDefault(); // stop submit
            showToast("Passwords do not match", false);
            return false;
        }

        signup();
    });

    $('#showpassword').on('click', function () {
        ShowPassword();

    });


});

function signup() {
    var username = $("#username").val();
    var password = $("#password").val();


    $.ajax({
        url: '/auth/SignupUser',
        type: 'POST',
        data: {
            Username: $("#username").val(),
            Password: $("#password").val()
        },
        success: function (response) {
            showToast(response.message, true);
            $("#username").val('');
            $("#password").val('');
            $("#Confirmpassword").val('');
        },
        error: function (xhr) {
            showToast(xhr.responseJSON?.message || "Server error", false);
        }

    });
}

function ShowPassword() {
    const passwordField = $('#Confirmpassword');
    const icon = $('#showpassword');

    if (passwordField.attr('type') === 'password') {
        passwordField.attr('type', 'text');
        icon.removeClass('fa-eye').addClass('fa-eye-slash');
    } else {
        passwordField.attr('type', 'password');
        icon.removeClass('fa-eye-slash').addClass('fa-eye');
    }
}


