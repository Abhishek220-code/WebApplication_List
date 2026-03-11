$(document).ready(function () {
    $("#loginForm").submit(function (e) {
        e.preventDefault();
        login();
    });

    $('#showpassword').on('click', function () {
        ShowPassword();

    });

});

function login() {

    $.ajax({
        url: '/auth/Login',
        type: 'POST',
        data: {
            Username: $("#username").val(),
            Password: $("#password").val(),
            RememberMe: $("#rememberMe").is(":checked"),
        },
        success: function (res) {
            window.location.href = "/User/List";
        },
        error: function (xhr) {
            alert(xhr.responseJSON?.message || "Login failed");
        },
    });
}

function ShowPassword() {
    const passwordField = $('#password');
    const icon = $('#showpassword');

    if (passwordField.attr('type') === 'password') {
        passwordField.attr('type', 'text');
        icon.removeClass('fa-eye').addClass('fa-eye-slash');
    } else {
        passwordField.attr('type', 'password');
        icon.removeClass('fa-eye-slash').addClass('fa-eye');
    }
}
