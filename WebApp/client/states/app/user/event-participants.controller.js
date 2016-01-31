app.controller('EventParticipantsController', function ($scope, $mdDialog, $log, $msUI, utilityService, siteService, event, eventSession) {
  $log.debug('Loading EventParticipantsController...');

  $scope.searchHandler = siteService.model.eventParticipants.search;
  $scope.demandParticipantGroup = siteService.demandParticipantGroup;
  $scope.demandEventSession = siteService.demandEventSession;


  $scope.event = event;
  // (optional - provided when we're looking at just one session, null when we're not)
  $scope.eventSession = eventSession;


  $scope.sortOptions = [
    { name: 'First Name', serverTerm: 'Participant.FirstName', clientFunction: utilityService.compareByProperties('firstName', 'id') },
    { name: 'Last Name', serverTerm: 'Participant.LastName', clientFunction: utilityService.compareByProperties('lastName', 'id') },
    //!! this is currently broken - as we don't really want to sort by the ParticipantGroup ID
    { name: 'School', serverTerm: 'Participant.ParticipantGroup.Name', clientFunction: utilityService.compareByProperties('participantGroupName', 'id') },
    { name: 'Grade', serverTerm: 'ExEventParticipant.item.Grade', clientFunction: utilityService.compareByProperties('grade', 'id') }
  ];

  var filterByStateFactory = function (includeState) {
    var includeStateLocal = includeState;
    return function (item) {
      return item.state === includeStateLocal;
    };
  };

  $scope.filterOptions = [
    //{ name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
    //{ name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },
    { name: 'All' }
  ];

  //!! fixup our event filter. Need a more robust way to handle this

  // Most restrictive to least restrictive
  if (eventSession) {
    // filter to one EventSession
    var serverTerm = '$eventSession:' + $scope.eventSession.id;
    var clientFunction = utilityService.filterByPropertyValue('eventSessionID', $scope.eventSession.id);
    $scope.filterOptions[0] = { name: 'EventSession', serverTerm: serverTerm, clientFunction: clientFunction }
  } else if (event) {
    // filter to one Event
    var serverTerm = '$event:' + $scope.event.id;
    var clientFunction = utilityService.filterByPropertyValue('eventID', $scope.event.id);
    $scope.filterOptions[0] = { name: 'Event', serverTerm: serverTerm, clientFunction: clientFunction }
  } else {
    // no filtering
  }



  $scope.searchViewOptions = {
    sort: $scope.sortOptions[0],
    filter: $scope.filterOptions[0]
  };



  $scope.checkIn = function (eventParticipant) {
    siteService.checkInEventParticipant(eventParticipant)
    .then(function (successData) {
      // success
      return successData;
    }, function (failureData) {
      // failure
      $log.debug(failureData.errorMessage);
      return failureData;
    });
  };

  $scope.checkOut = function (eventParticipant) {
    siteService.checkOutEventParticipant(eventParticipant)
    .then(function (successData) {
      // success
      return successData;
    }, function (failureData) {
      // failure
      $log.debug(failureData.errorMessage);
      return failureData;
    });
  };




  $scope.download = function () {
    var query = {
      type: 'tenants'
    };
    utilityService.download(query);
  };

});
