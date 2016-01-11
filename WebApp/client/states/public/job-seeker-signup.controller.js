app.controller('JobSeekerSignupController', function ($scope, $log, $filter, $state, utilityService, siteService) {
  $log.debug("Loading JobSeekerSignupController...");

  $scope.formData = {};
  $scope.passwordInputType = "password";

  $scope.demoAutoFill = function () {
    utilityService.generateRandomContact()
      .then(function (randomContactData) {
        $scope.formData.firstName = $filter('capitalize')(randomContactData.name.first);
        $scope.formData.lastName = $filter('capitalize')(randomContactData.name.last);
        $scope.formData.emailAddress = randomContactData.email;
        $scope.formData.cellPhone = randomContactData.cell;

        $scope.formData.password = randomContactData.password;
        $scope.passwordInputType = "text";

        $scope.formData.profilePhotoUrl = randomContactData.picture;
      });
  };

  $scope.createAccount = function () {
    $scope.working = true;
    siteService.createJobSeeker($scope.formData)
    .then(function (newUserCred) {
      // success (create job seeker)
      utilityService.signIn(newUserCred)
      .then(function () {
        // success (sign in)
        $state.go('app.job-seeker.onboard.intro');
      }, function () {
        // failure (sign in) (shouldn't happen)
        alert("nope");
      });
    }, function (failureData) {
      // failure (create job seeker)
      alert(failureData.errorMessage);
    });
  };
});
