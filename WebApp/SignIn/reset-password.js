
app.controller('ResetPasswordController', function ($scope, $modal, $http, $window, utilityService) {

    $scope.resetPassword = function (authCode, newPassword) {
        utilityService.resetPassword(authCode, newPassword)
            .then(function () {
                $window.location.href = '/';
            });
    };

    // init
    // (nothing to do)
});