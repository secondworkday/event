app.controller('TenantDashboardController', function ($scope, $log, $msUI, utilityService, siteService) {
  $log.debug('Loading TenantDashboardController...');

  $scope.closeMyTask = function (myTask, $event) {
    utilityService.closeMyTask(myTask);
    // $msUI.showToast("Task completed, high five!");
    $log.debug("Task completed.");
  };

});
