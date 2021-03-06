app.controller('VolunteerSignInController', function ($scope, $state, $window, $mdToast, $mdDialog, $log, webUtilityService, utilityService, siteService) {
  $log.debug('Loading VolunteerSignInController...');

  $scope.stateData = $state.current.data;

  // this triggers the spinner for 'signing you in...'
  $scope.working = false;

  $scope.signIn = function () {
    $scope.invalidPIN = false;
    $scope.working = true;
    utilityService.signIn($scope.formData)
      .then(function () {
        // success
        $window.location.href = '/check-in';
        // $state.go('app.user.events', {}, { reload: true });
        // $state.go('app.system.users', {}, { reload: true });
      }, function (failureData) {
        // failure
        //alert(failureData);
        $scope.working = false;
        $scope.invalidPIN = true;
      });
  };

});
