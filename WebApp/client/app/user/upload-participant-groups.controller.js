app.controller('UploadParticipantGroupsDialogController', function ($scope, $mdDialog, $log, $msUI, utilityService, siteService, event) {
  $log.debug('Loading UploadParticipantGroupsDialogController...');

  $scope.participantGroups = siteService.model.participantGroups;
  $scope.event = event;

  $scope.hide = function () {
    $mdDialog.hide();
  };
  $scope.cancel = function () {
    $mdDialog.cancel();
  };

  $scope.dataErrors = {
    numberOfErrors: 0
  }

  $scope.paste = function ($event) {
    var pastedTextData = $event.originalEvent.clipboardData.getData('text/plain');

    siteService.parseParticipantGroups(event, pastedTextData)
    .then(function (successData) {
      // success
      $scope.uploadData = {
        //participantGroupID: 1,
        itemsData: successData.ResponseData
      };
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

  $scope.uploadParticipantGroups = function () {

    var uploadData = {
      participantGroupID: 1,
      itemsData: [
        { firstName: 'betty', lastName: 'rubbles' },
        { firstName: 'barney', lastName: 'rubble', participantGroupName: "Stella Schola" },
        { firstName: 'fred', lastName: 'flintstone', participantGroupID: 3 }]
    };

    siteService.uploadParticipantGroups(event, $scope.uploadData);
    $mdDialog.hide();
  };

});