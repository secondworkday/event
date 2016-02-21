app.controller('EventHistoryController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading EventHistoryController...');

  $scope.sampleActivities = [
    {
      type: "bulkAddParticipants",
      amount: "34",
      target: "event",
      actor: "Jerry Seinfeld",
      timeStamp: "2016-02-20T19:21:34.525Z"
    },
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
    {
      type: "bulkDeleteParticipants",
      amount: "12",
      target: "event",
      actor: "Elaine Bennett",
      timeStamp: "2016-02-12T19:21:34.525Z"
    },
  ];

});
