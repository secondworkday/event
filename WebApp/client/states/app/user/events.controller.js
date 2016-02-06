app.controller('EventsController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading EventsController...');

  $scope.samplePastEvents = [
    {
      name: "Spring 2015 A",
      startDate: "4/1/2015",
      endDate: "4/7/2015",
      expectedParticipantCount: 999,
      actualParticipantCount: 888
    },
    {
      name: "Spring 2015 B",
      startDate: "4/8/2015",
      endDate: "4/16/2015",
      expectedParticipantCount: 999,
      actualParticipantCount: 888
    },
    {
      name: "Fall 2015 A",
      startDate: "9/1/2015",
      endDate: "9/7/2015",
      expectedParticipantCount: 999,
      actualParticipantCount: 888
    },
    {
      name: "Fall 2015 B",
      startDate: "9/4/2015",
      endDate: "9/12/2015",
      expectedParticipantCount: 999,
      actualParticipantCount: 888
    }
  ];

});
