app.controller('NoShowReportDialogController', function ($scope, $window, $translate, $log, $filter, $state, $mdDialog, $msUI, utilityService, siteService, event, eventSessionsIndex) {

  $scope.eventSessionsIndex = eventSessionsIndex;

  // for loading items from the model
  $scope.eventSessions = siteService.model.eventSessions;
  $scope.participantGroups = siteService.model.participantGroups;
  $scope.eventParticipants = siteService.model.eventParticipants;

  $scope.eventSessionIDChanged = function () {
    // ** Build the cascading drop-down containing the participantGroups for selected eventSession

    // Count our slacker no-shows EventParticipants for this EventSession, gathered by ParticipantGroup
    var noShowSessionParticipantGroupsCounts = {};
    var noShowSessionParticipantGroupsIndex = [];
    angular.forEach(siteService.eventParticipants.index, function (eventParticipantID) {
      var eventParticipant = siteService.eventParticipants.hashMap[eventParticipantID];
      if ($scope.formData.eventSessionID == eventParticipant.eventSessionID && !eventParticipant.checkInTimestamp) {
        if (!noShowSessionParticipantGroupsCounts[eventParticipant.participantGroupID]) {
          noShowSessionParticipantGroupsCounts[eventParticipant.participantGroupID] = 1;
          noShowSessionParticipantGroupsIndex.push(eventParticipant.participantGroupID);
        } else {
          noShowSessionParticipantGroupsCounts[eventParticipant.participantGroupID]++;
        }
      }
    });
    // Ensure we've loaded each of these ParticipantGroups into our model
    siteService.participantGroups.ensure(noShowSessionParticipantGroupsIndex)
    .then(function () {
      // all done - flag the UI we can show 'em
      $scope.noShowSessionParticipantGroupsCounts = noShowSessionParticipantGroupsCounts;
      $scope.noShowSessionParticipantGroupsIndex = noShowSessionParticipantGroupsIndex;
    });
  };

  function printEventParticipantInfo(eventParticipant) {
    var ep = eventParticipant;
    return $filter('uppercase')(ep.lastName) + ", " + ep.firstName + printLevel(ep.level);
  }

  function printLevel(level) {
    if (level) {
      return ", " + $translate.instant("PARTICIPANT_LEVEL") + " " + level;
    }
    return "";
  }

  function printNote(notes) {
    if (notes) {
      return " - " + notes;
    }
    return "";
  }

  $scope.generateNoShowReport = function (eventSessionID, participantGroupID) {
    $log.debug("Generate no show for " + eventSessionID);

    var searchExpression = "$eventSession:" + eventSessionID
      + " $participantGroup:" + participantGroupID
      + " $notCheckedIn"
    ;

    siteService.model.eventParticipants.search(searchExpression, "Participant.LastName", 0, 99999)
      .then(function (data) {
        var participantGroup = siteService.model.participantGroups.hashMap[participantGroupID];
        var eventSession = siteService.model.eventSessions.hashMap[eventSessionID];

        var eventStartDate = moment.utc(eventSession.startDate).toDate(); // convert to local time

        //var eventDetailsSection = "EVENT DETAILS:\n"
        //  + eventSession.location + "\n"
        //  + moment(eventStartDate).format("MMMM Do YYYY, h:mm a") + "\n"
        //  + eventSession.locationStreetAddress + "\n"
        //  + eventSession.locationCity + " " + eventSession.locationState + " " + eventSession.locationZipCode
        //;

        var participantSection = "";

        for (i = 0; i < data.ids.length; i++) {
          participantSection += printEventParticipantInfo(data.hashMap[data.ids[i]]) + "\n";
        }


        //var emailSubject = "No Show report for " + eventSession.location + ", " + moment(eventStartDate).format("MMMM Do YYYY, h:mm a");
        var emailSubject = "No Show report for " + $translate.instant("PROGRAM_NAME");
        var emailBody = "Dear " + participantGroup.contactName + ","
          + "\n\n"
          + "Listed below are the students that did not shop at " + $translate.instant("PROGRAM_NAME") + " last night."
          + "\n\n"
            //+ eventDetailsSection
            //+ "\n\n"
          + participantSection
          + "\n"
          + "Regards,\n"
          + utilityService.model.authenticatedUser.firstName
          + "\n\n"
        ;

        var link = "mailto:" + participantGroup.primaryEmail
               + "?subject=" + escape(emailSubject)
               + "&body=" + escape(emailBody)
        ;

        $window.location.href = link;

        $scope.cancel();
      });
  };

  $scope.cancel = function () {
    $mdDialog.cancel();
  };





});