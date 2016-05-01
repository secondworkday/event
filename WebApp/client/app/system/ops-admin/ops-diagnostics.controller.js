app.controller('OpsDiagnosticsController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading OpsDiagnosticsController...');

  $scope.raiseDiagnosticEvent = utilityService.raiseDiagnosticEvent;
  $scope.generateCriticalEventLogItem = utilityService.generateCriticalEventLogItem;


});
