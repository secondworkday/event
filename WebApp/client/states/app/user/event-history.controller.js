app.controller('EventHistoryController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading EventHistoryController...');

  $scope.sampleActivities = [
    {
      type: "New Session",
      description: "Added a new Session to this Event",
      actor: "Jerry Seinfeld",
      date: "Date"
    }
  ];

});
