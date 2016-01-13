app.controller('SignInController', function ($scope, $state, $mdToast, $log, webUtilityService, utilityService, siteService) {
  $log.debug('Loading SignInController...');

  $scope.stateData = $state.current.data;

  $scope.signIn = function (credentials) {
    utilityService.signIn(credentials)
      .then(function () {
        // success
        $state.go('app.home', {}, { reload: true });
        // $state.go('app.system.users', {}, { reload: true });
      }, function (failureData) {
        // failure
        alert(failureData);
      });
  };


  //!! Ok - there must be a way better way than passing fnHide as a parameter here. I'm missing something really basic.
  $scope.sendPasswordResetEmail = function (email, fnHide) {
    webUtilityService.sendResetPasswordEmail(email)
      .then(function (successData) {
        $mdToast.showSimple("Yessir.");

        //var myAlert = $alert({ title: 'Password Reset', content: 'Password reset email sent to ' + email + '.', placement: 'top', type: 'info', show: true });
        //fnHide();
      }, function (failureData) {

        if (failureData.StatusCode == 403) {
          $mdToast.showSimple(failureData.errorMessage || failureData.StatusCodeDescription);

          //var myAlert = $alert({ title: 'Password Reset', content: failureData.StatusCodeDescription, placement: 'top', container: 'body', type: 'info', duration: 3, show: true });
          //fnHide();
          return;
        }

        //!! TODO this should be displayed as an error on the email field
        //var myAlert = $alert({ title: 'Password Reset', content: failureData.StatusCodeDescription, placement: 'top', type: 'info', show: true });

        $mdToast.showSimple(failureData.errorMessage);

      });
  };

});
