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

  $scope.checkedInCount = 0;
  $scope.expectedCount = 99;
  $scope.timeRemaining = "3h 45m";

  $scope.sampleParticipants = [
    { firstName: "Johnny", lastName: "Walker", school: "Eastside Elementary", grade: "3rd", status: "expected" },
    { firstName: "Sam", lastName: "Adams", school: "Eastside Elementary", grade: "2nd", status: "expected" },
    { firstName: "Alize", lastName: "Blue", school: "Westside Elementary", grade: "4th", status: "expected" }
  ];

  $scope.checkIn = function (participant) {
    participant.status = "checkedIn";
    $scope.checkedInCount++;
    $scope.expectedCount--;
  };

  $scope.undoCheckIn = function (participant) {
    participant.status = "expected";
    $scope.checkedInCount--;
    $scope.expectedCount++;
  };





  //!! we rock so good. If this session is active, we want to track Participants - who hasn't checked it, who has checked in, and who's checked out.

  eventSession.allParticipants = {
    index: [],
    sort: utilityService.compareByProperties('id'),
    filter: function (item) {
      return item.eventSessionID === $scope.eventSession.id;
    }
  };

  eventSession.expectedParticipants = {
    index: [],
    sort: utilityService.compareByProperties('id'),
    filter: function (item) {
      return item.eventSessionID === $scope.eventSession.id && !item.checkInTimestamp;
    }
  };
  eventSession.checkedInParticipants = {
    index: [],
    sort: utilityService.compareByProperties('id'),
    filter: function (item) {
      return item.eventSessionID === $scope.eventSession.id && item.checkInTimestamp && !item.checkOutTimestamp;
    }
  };
  eventSession.checkedOutParticipants = {
    index: [],
    sort: utilityService.compareByProperties('id'),
    filter: function (item) {
      return item.eventSessionID === $scope.eventSession.id && item.checkInTimestamp && item.checkOutTimestamp;
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



  /*
  activeUsersIndexer: {
      index: [],
      sort: self.localeCompareByPropertyThenByID('displayName'),
      filter: function (item) {
        return item.state === USER_STATE.active;
      }
  },
  disabledUsersIndexer: {
    index: [],
    sort: self.localeCompareByPropertyThenByID('displayName'),
    filter: function (item) {
      return item.state === USER_STATE.disabled;
    }
*/




});
