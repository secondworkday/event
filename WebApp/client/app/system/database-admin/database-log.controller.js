app.controller('DatabaseLogController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading DatabaseLogController...');

  $scope.searchDatabaseLog = utilityService.searchDatabaseLog;

    $scope.$on("$destroy", function () {
        // unrequest server notifications
        utilityService.untrackDatabaseLog();
    });

    // init

    // request server notifications
    utilityService.trackDatabaseLog();

});
