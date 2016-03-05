app.controller('EventSessionController', function ($scope, $translate, $log, $state, $mdDialog, $msUI, utilityService, siteService, event, eventSession) {
  $log.debug('Loading EventSessionController...');
  $log.debug('Loading ' + $state.name + '...');

  //$scope.model = utilityService.model;
  $scope.event = event;
  $scope.eventSession = eventSession;

  $scope.participantGroups = siteService.model.participantGroups;
  $scope.eventParticipants = siteService.model.eventParticipants;

  $scope.searchParticipantGroups = siteService.model.participantGroups.search;
  $scope.searchEventParticipants = siteService.model.eventParticipants.search;
  $scope.searchEvents = siteService.model.events.search;


  var baseParticipantFilter = function (item) {
    return item.eventSessionID === $scope.eventSession.id;
  };



  //!! we rock so good. If this session is active, we want to track Participants - who hasn't checked it, who has checked in, and who's checked out.

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


  var eventSessionID = $scope.eventSession.id;
  $scope.searchEventParticipants("$eventSession:" + eventSessionID, "", 0, 99999);


});
