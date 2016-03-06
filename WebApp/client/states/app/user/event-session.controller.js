app.controller('EventSessionController', function ($scope, $translate, $log, $state, $mdDialog, $msUI, utilityService, siteService, event, eventSession) {
  $log.debug('Loading EventSessionController...');
  $log.debug('Loading ' + $state.name + '...');

  $scope.event = event;
  $scope.eventSession = eventSession;



  // ** Create some EventParticipant indexers - so we can track our check-in & check-out progress through the EventSession

  var baseParticipantFilter = function (item) {
    return item.eventSessionID === $scope.eventSession.id;
  };

  eventSession.allParticipants = {
    index: [],
    sort: utilityService.compareByProperties('id'),
    baseFilter: baseParticipantFilter
  };

  eventSession.expectedParticipants = {
    index: [],
    sort: utilityService.compareByProperties('id'),
    baseFilter: baseParticipantFilter,
    selectFilter: function (item) {
      return !item.checkInTimestamp;
    }
  };
  eventSession.checkedInParticipants = {
    index: [],
    sort: utilityService.compareByProperties('id'),
    baseFilter: baseParticipantFilter,
    selectFilter: function (item) {
      return item.checkInTimestamp && !item.checkOutTimestamp;
    }
  };
  eventSession.checkedOutParticipants = {
    index: [],
    sort: utilityService.compareByProperties('id'),
    baseFilter: baseParticipantFilter,
    selectFilter: function (item) {
      return item.checkInTimestamp && item.checkOutTimestamp;
    }
  };


  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.allParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.expectedParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.checkedInParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.checkedOutParticipants);

  $scope.$on("$destroy", function () {
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.allParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.expectedParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.checkedInParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.checkedOutParticipants);
  });


  siteService.getEventSessionVolunteerAuthInfo($scope.eventSession)
  .then(function (authInfo) {
    $scope.authInfo = authInfo;
  });

  // It's our responsibility to pre-load all the EventParticipants in scope
  siteService.model.eventParticipants.search("$eventSession:" + eventSession.id, "", 0, 99999);
});