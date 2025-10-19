function token() {
    return $('input[name="__RequestVerificationToken"]').val();
}

function loginAccount(userInput) {
    userInput.__RequestVerificationToken = token();
    $.ajax({
        type: "POST",
        url: "/Account/LoginToSystem", 
        data: userInput,
        dataType: 'json',

        success: function (res) {
            if (res.status === 'success') {
                location.href = '/Home/index';
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi đăng nhập',
                    text: res.message
                });
            }
        },
        error: function () {
            x = 3;
        }

    });
}

function registerAccount(userInput) {
    userInput.__RequestVerificationToken = token();
    $.ajax({
        type: "POST",
        url: "Account/RegisterToSystem",
        data: userInput,
        dataType: 'json',

        success: function (res) {
            if (res.status === 'success') {
                Swal.fire({
                    icon: 'success',
                    title: 'Đăng ký thành công',
                    text: res.messagge
                }).then(() => location.href = '/Login');
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi đăng ký',
                    text: res.message
                });
            }
        },

        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi hệ thống',
                text: 'Vui lòng thử lại sau'
            });
        }
    });
}

document.addEventListener('DOMContentLoaded', function () {
    //login form
    $(document).off('submit', '#login_form');
    $(document).on('submit', '#login_form', function (e) {
        e.preventDefault();
        loginAccount({
            username: $('#usernameInput').val(),
            password: $('#passwordInput').val()
        });
    })
    //register form
    $(document).off('submit', '#register_form');
    $(document).on('submit', '#register_form', function (e) {
        e.preventDefault();
        registerAccount({
            fullname: $('#fullname').val(),
            email: $('#email').val(),
            username: $('#username').val(),
            password: $('#password').val(),
            confirmPassword: $('#confirmPassword').val()
        });
    })
})