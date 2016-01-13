app.controller('PublicController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading PublicController...');

  $scope.reloadPage = function () {
    utilityService.restartConnection();
  }

});
