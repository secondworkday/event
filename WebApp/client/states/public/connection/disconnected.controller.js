app.controller('DisconnectedController', function ($scope, utilityService, siteService) {

  $scope.reloadPage = function () {
    utilityService.restartConnection();
  }

});
