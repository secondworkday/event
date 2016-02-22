app.controller('ActivityLogController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading ActivityLogController...');

  $scope.searchActivityLog = utilityService.model.activityLog.search;

  $scope.demandUser = utilityService.demandUser;


  $scope.searchViewOptions = {};


  // Establish our Base filtering (evaluatuating in order of most restrictive to least restrictive)
  if ($scope.eventSession) {
    // filter to one EventSession
    $scope.searchViewOptions.baseFilter = { serverTerm: '$eventSession:' + $scope.eventSession.id, clientFunction: utilityService.filterByPropertyValue('eventSessionID', $scope.eventSession.id) };
  } else if ($scope.event) {
    // filter to one Event
    $scope.searchViewOptions.baseFilter = { serverTerm: '$event:' + $scope.event.id, clientFunction: utilityService.filterByPropertyValue('eventID', $scope.event.id) };
  } else {
    // no filtering
  }


  $scope.sortOptions = [
    { name: 'All', serverTerm: '', clientFunction: utilityService.compareByProperties('id') }

    //{ name: 'First Name', serverTerm: 'Participant.FirstName', clientFunction: utilityService.compareByProperties('firstName', 'id') },
    //{ name: 'Last Name', serverTerm: 'Participant.LastName', clientFunction: utilityService.compareByProperties('lastName', 'id') },
    //!! this is currently broken - as we don't really want to sort by the ParticipantGroup ID
    //{ name: 'School', serverTerm: 'Participant.ParticipantGroup.Name', clientFunction: utilityService.compareByProperties('participantGroupName', 'id') },
    //{ name: 'Grade', serverTerm: 'ExEventParticipant.item.Grade', clientFunction: utilityService.compareByProperties('grade', 'id') }
  ];

  $scope.searchViewOptions.sort = $scope.sortOptions[0];

  $scope.filterOptions = [
    //{ name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
    //{ name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },
    { name: 'All' }
  ];






  $scope.sampleActivities = [
    {
      type: "bulkAddParticipants",
      amount: "34",
      target: "event",
      actor: "Jerry Seinfeld",
      timeStamp: "2016-02-20T19:21:34.525Z"
    },
/*
    {
      type: "singleAddParticipant",
      participant: { firstName: "Jon", lastName: "Snow", participantGroup: "Parkside Elementary"},
      target: "event",
      actor: "George Constanza",
      timeStamp: "2016-02-19T19:21:34.525Z"
    },
    {
      type: "singleAddParticipant",
      participant: { firstName: "Daenerys", lastName: "Targaryen", participantGroup: "Parkside Elementary"},
      target: "event",
      actor: "George Constanza",
      timeStamp: "2016-02-19T19:21:34.525Z"
    },
    {
      type: "singleAddParticipant",
      participant: { firstName: "Arya", lastName: "Stark", participantGroup: "Greendale Middle School"},
      target: "event",
      actor: "George Constanza",
      timeStamp: "2016-02-19T19:21:34.525Z"
    },
    {
      type: "singleAddParticipant",
      participant: { firstName: "Tyrion", lastName: "Lannister", participantGroup: "Greendale Middle School"},
      target: "event",
      actor: "George Constanza",
      timeStamp: "2016-02-19T19:21:34.525Z"
    },
    {
      type: "singleAddParticipant",
      participant: { firstName: "Sansa", lastName: "Stark", participantGroup: "Parkside Elementary"},
      target: "event",
      actor: "George Constanza",
      timeStamp: "2016-02-19T19:21:34.525Z"
    },
*/
    {
      type: "bulkDeleteParticipants",
      amount: "12",
      target: "event",
      actor: "Elaine Bennett",
      timeStamp: "2016-02-12T19:21:34.525Z"
    },
  ];

});
