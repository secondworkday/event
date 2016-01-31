app.controller('EventSessionController', function ($scope, $mdDialog, $log, $msUI, utilityService, siteService, event, eventSession) {
  $log.debug('Loading EventSessionController...');

  //$scope.model = utilityService.model;
  $scope.event = event;
  $scope.eventSession = eventSession;


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
