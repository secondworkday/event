app.controller('EventsController', function ($scope, $log, moment, utilityService, siteService, eventSessions) {

  $log.debug('Loading EventsController...');

  //$scope.eventSessions = eventSessions;

  // EventSessions for a specific Event by ID
  function getEventSessionsIndex(eventID) {
    return $.map(eventSessions.hashMap, function (value, index) {
      if (eventID == value.eventID) {
        return value.id;
      }
    });
  }

  $scope.getEventDatesString = function (eventID) {
    var eventSessionsIndex = getEventSessionsIndex(eventID);

    var startDate, endDate;

    // find the first and last Event Session (data-wise)
    for (i = 0; i < eventSessionsIndex.length; i++) {
      var eventSession = eventSessions.hashMap[eventSessionsIndex[i]];
      if (startDate == null) {
        startDate = moment(eventSession.startDate);
      }
      else {
        startDate = moment(eventSession.startDate) < startDate ? moment(eventSession.startDate) : startDate;
      }
      if (endDate == null) {
        endDate = moment(eventSession.endDate);
      }
      else {
        endDate = moment(eventSession.endDate) > endDate ? moment(eventSession.endDate) : endDate;
      }
    }


    // form the string to display
    if (startDate == null && endDate == null) {
      return "";
    }

    var firstPart = startDate.format("MMM D");
    var secondPart = "";

    if (startDate.month() == endDate.month()) {
      if (startDate.day() == endDate.day()) {
        // Feb 26
      }
      else {
        // Feb 26 - 28
        secondPart = " - " + endDate.format("D");
      }
    }
    else {
      // Feb 26 - Mar 6 
      secondPart = " - " + endDate.format("MMM D");
    }

    return firstPart + secondPart;
  }

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
