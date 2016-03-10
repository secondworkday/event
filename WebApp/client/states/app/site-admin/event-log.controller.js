app.controller('EventLogController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading EventLogController...');

  $scope.searchEventLog = utilityService.model.eventLog.search;

  $scope.searchViewOptions = {
    sort: { name: 'Recent First', serverTerm: '', clientFunction: utilityService.compareByProperties('-firstOccurenceTimestamp', '-id') },
    selectFilter: { name: 'All' }
  };

  $scope.$on("$destroy", function () {
    // unrequest server notifications
    utilityService.untrackEventLog();
  });

  // init

  // request server notifications
  utilityService.trackEventLog();

});
