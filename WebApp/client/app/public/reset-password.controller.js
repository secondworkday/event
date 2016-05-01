(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('ResetPasswordController', ResetPasswordController);

    ResetPasswordController.$inject = ['$scope', '$stateParams', '$window', '$msUI', 'utilityService', 'webUtilityService'];

    /* @ngInject */
    function ResetPasswordController($scope, $stateParams, $window, $msUI, utilityService, webUtilityService) {
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

          $scope.failureData = failureData;

          $msUI.showToast(failureData.errorMessage || failureData.statusText);
        });
      };

      //!! something isn't working here, i got an error in the console:
      //!! TypeError: Cannot read property 'done' of null
      $scope.resendPasswordResetEmail = function(emailAddress) {
        webUtilityService.sendResetPasswordEmail(emailAddress)
          .then(function (successData) {
            // success
            $msUI.showToast("Password reset email sent.");
            return successData;
          }, function (failureData) {
            // failure
            if (failureData.StatusCode == 403) {
              $msUI.showToast(failureData.errorMessage || failureData.StatusCodeDescription);
              return;
            }
            $msUI.showToast(failureData.errorMessage);
            $log.debug(failureData.errorMessage);
            return failureData;
          });
      };
    }
})();
