app.controller('GenerateRemindersDialogController', function ($scope, $window, $translate, $log, $filter, $state, $mdDialog, $msUI, utilityService, siteService, event, eventSessionsIndex) {

  $scope.eventSessionsIndex = eventSessionsIndex;

  // for loading items from the model
  $scope.eventSessions = siteService.model.eventSessions;
  $scope.participantGroups = siteService.model.participantGroups;
  $scope.eventParticipants = siteService.model.eventParticipants;

  $scope.eventSessionIDChanged = function () {
    // ** Build the cascading drop-down containing the participantGroups for selected eventSession

    // Count our slacker no-shows EventParticipants for this EventSession, gathered by ParticipantGroup
    var sessionParticipantGroupsCounts = {};
    var sessionParticipantGroupsIndex = [];
    angular.forEach(siteService.eventParticipants.index, function (eventParticipantID) {
      var eventParticipant = siteService.eventParticipants.hashMap[eventParticipantID];
      if ($scope.formData.eventSessionID == eventParticipant.eventSessionID) {
        if (!sessionParticipantGroupsCounts[eventParticipant.participantGroupID]) {
          sessionParticipantGroupsCounts[eventParticipant.participantGroupID] = 1;
          sessionParticipantGroupsIndex.push(eventParticipant.participantGroupID);
        } else {
          sessionParticipantGroupsCounts[eventParticipant.participantGroupID]++;
        }
      }
    });
    // Ensure we've loaded each of these ParticipantGroups into our model
    siteService.participantGroups.ensure(sessionParticipantGroupsIndex)
    .then(function () {
      // all done - flag the UI we can show 'em
      $scope.sessionParticipantGroupsCounts = sessionParticipantGroupsCounts;
      $scope.sessionParticipantGroupsIndex = sessionParticipantGroupsIndex;
    });
  };

  $scope.cancel = function () {
    $mdDialog.cancel();
  };

  $scope.downloadReminderForm = function (eventSessionID, participantGroupID) {

    var ev = $scope.eventSessions.hashMap[eventSessionID];
    var pg = $scope.participantGroups.hashMap[participantGroupID];

    var filename = "OSB Reminder Form - "
            + ev.name + " " + $filter('date')(ev.startDate, 'MM-dd-yyyy')
            + " - " + pg.name + " (" + pg.contactName + ")"
    ;

    var query = {
      type: 'reminderFormForEventParticipant',
      participantGroupID: participantGroupID,
      eventSessionID: eventSessionID,
      pdfFilename: filename
    };

    if ($scope.formData.sendEmail) {
      var participantGroup = siteService.model.participantGroups.hashMap[participantGroupID];

      var emailSubject = "Reminder Form - Assistance League of the Eastside ~ Operation School Bell";
      var emailBody = "Thank you very much for sending in your student list for the Operation School Bell shopping events."
        + "\n\n"
        + "Attached is a PDF file with a pre-filled check-in form which includes date, time and location for each of your students. Please make sure to print these out and distribute to the student a few days before the scheduled shopping event."
        + "\n\n"
        + "Please ask your student/family to bring this form to the check in table at the shopping event."
        + "\n\n"
        + "If you have any questions, please contact me."
        + "\n\n"
        + "Thank you again for all that you do for Assistance League of the Eastside and the students in our community."
        + "\n\n"
      ;

      var link = "mailto:" + participantGroup.primaryEmail
               + "?subject=" + escape(emailSubject)
               + "&body=" + escape(emailBody)
      ;

      var wi = window.open(link);

      setTimeout(function () {
        wi.location.href = "/Download.ashx?" + $.param(query);
      }, 500);
    }
    else {
      utilityService.download(query);
    }

    var selectedData = {
      participantGroupID: participantGroupID,
      sendEmail: $scope.formData.sendEmail
    }

    $mdDialog.hide(selectedData);
  };

  // init
  $scope.formData = {
    sendEmail: true
  };



});