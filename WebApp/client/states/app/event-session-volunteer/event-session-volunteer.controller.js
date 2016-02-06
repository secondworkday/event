app.controller('EventSessionVolunteerController', function ($scope, $translate, $log, $state, $mdDialog, $msUI, utilityService, siteService) {
  $log.debug('Loading EventSessionVolunteerController...');
  $log.debug('Loading ' + $state.name + '...');

  $scope.participantGroups = siteService.model.participantGroups;


  $scope.searchParticipantGroups = siteService.model.participantGroups.search;
  $scope.searchParticipants = siteService.model.participants.search;
  $scope.searchEvents = siteService.model.events.search;

  $scope.checkedInCount = 0;
  $scope.expectedCount = 99;
  $scope.timeRemaining = "3h 45m"

  $scope.sampleParticipants = [
    { firstName: "Johnny", lastName: "Walker", school: "Eastside Elementary", grade: "3rd", status: "expected"},
    { firstName: "Sam", lastName: "Adams", school: "Eastside Elementary", grade: "2nd", status: "expected"},
    { firstName: "Alize", lastName: "Blue", school: "Westside Elementary", grade: "4th", status: "expected"}
  ];

  $scope.checkIn = function(participant) {
    participant.status = "checkedIn";
    $scope.checkedInCount++;
    $scope.expectedCount--;
  };

  $scope.undoCheckIn = function(participant) {
    participant.status = "expected";
    $scope.checkedInCount--;
    $scope.expectedCount++;
  };

  $scope.sortOptions = [
    { name: 'Name', serverTerm: 'item.name', clientFunction: utilityService.localeCompareByPropertyThenByID('name') },
  ];

  var filterByStateFactory = function (includeState) {
      var includeStateLocal = includeState;
      return function (item) {
          return item.state === includeStateLocal;
      };
  };

  $scope.filterOptions = [
    { name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
    { name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },
    { name: 'All' }
  ];

  $scope.searchViewOptions = {
      sort: $scope.sortOptions[0],
      filter: $scope.filterOptions[2],
      participantGroupSearch: null
  };



  $scope.searchParticipantViewOptions = {
    sort: $scope.sortOptions[0],
    filter: $scope.filterOptions[2],
    participantSearch: null
  };


});
