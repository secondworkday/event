app.controller('UploadParticipantsDialogController', function ($scope, $mdDialog, $log, $msUI, $translate, utilityService, siteService, event, eventSessionsIndex) {
  $log.debug('Loading UploadParticipantsDialogController...');

  $scope.participantGroups = siteService.model.participantGroups;
  $scope.event = event;
  $scope.eventSessionsIndex = eventSessionsIndex;
  $scope.eventSessions = siteService.model.eventSessions;

  $scope.uploadData = {};

  $scope.hide = function () {
    $mdDialog.hide();
  };
  $scope.cancel = function () {
    $mdDialog.cancel();
  };

  $scope.dataErrors = {
    numberOfErrors: 0
  }

  $scope.clearData = function () {
    $scope.formData.participantTextInput = "";
    $scope.uploadData.itemsData = null;
  }

  $scope.paste = function ($event) {
    var pastedTextData = $event.originalEvent.clipboardData.getData('text/plain');

    siteService.parseEventParticipants(event, pastedTextData)
    .then(function (successData) {
      // success
      $scope.uploadData.itemsData = successData.ResponseData;
      //$scope.uploadData = {
      //  //participantGroupID: 1,
      //  itemsData: successData.ResponseData
      //};
      checkPresenceOfErrors();
      return successData;
    }, function (failureData) {
      // failure
      $log.debug(failureData.errorMessage);
      return failureData;
    });

  }

  function checkPresenceOfErrors() {
    $scope.dataErrors.numberOfErrors = 0;
    $.each($scope.uploadData.itemsData, function (index, value) {
      if (value.errors) {
        $scope.dataErrors.numberOfErrors++;
      }
    });
  }

  $scope.parseEventParticipants = function () {

    var parseData =
      "FirstName,LastName,Gender,School,Grade\r\n" +
      "Betty,Rubble,F,Stella Schola,5\r\n" +
      "\r\n" +
      "Harvey,Rubble,bogus,F,Stella Schola,5\r\n" +
      "\r\n" +
      "Barney,Rubble,M,Stella Schola,3\r\n" +
      "Fred,Flintstone,M,Stella Schola,6\r\n";

    siteService.parseEventParticipants(event, parseData)
    .then(function (successData) {
      // success
      $scope.uploadData.itemsData = successData.ResponseData;
      //$scope.uploadData = {
      //  itemsData: successData.ResponseData
      //};
      return successData;
    }, function (failureData) {
      // failure
      $log.debug(failureData.errorMessage);
      return failureData;
    });

  };

  $scope.uploadEventParticipants = function () {

    var sampleUploadData = {
      participantGroupID: 1,
      itemsData: [
        { firstName: 'betty', lastName: 'rubbles' },
        { firstName: 'barney', lastName: 'rubble', participantGroupName: "Stella Schola" },
        { firstName: 'fred', lastName: 'flintstone', participantGroupID: 3 }]
    };
    $msUI.showToast("Uploading " + $scope.uploadData.itemsData.length + " students", "bulkEventParticipantUpload");
    siteService.uploadEventParticipants(event, $scope.uploadData);
    $mdDialog.hide();
  };






  $scope.generateRandomParticipants = function (participantGroupID, numberOfParticipants) {
    siteService.generateRandomParticipants(participantGroupID, numberOfParticipants);
    $mdDialog.hide();
  };
  $scope.createParticipant = function (formData) {
    siteService.createParticipant(formData);
    $mdDialog.hide();
  }



});