
app.controller('ResetPasswordController', function ($scope, $stateParams, $window, $msUI, utilityService) {

  var authCode = $stateParams.authCode;

  $scope.resetPassword = function () {
    utilityService.resetPassword(authCode, $scope.password)
    .then(function (successData) {
      // success
      $window.location.href = '/';
    }, function (failureData) {
      // failure
      console.log("ResetPasswordController.resetPassword() failed", failureData);

      if (failureData.status == 403) {
        // Forbidden!!
      } else if (failureData.status == 404) {
        //!! Not found - where did ya get that code?
      }

      $msUI.showToast(failureData.errorMessage || failureData.statusText);
    });
  };
});