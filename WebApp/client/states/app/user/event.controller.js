app.controller('EventController', function ($scope, $log, $state, $mdDialog, $msUI, utilityService, siteService, event, eventSessionsIndex, eventParticipantsIndex, eventParticipantGroupsIndex) {
  $scope.event = event;
  $scope.eventSessionsIndex = eventSessionsIndex;
  $scope.eventParticipantsIndex = eventParticipantsIndex;
  $scope.eventParticipantGroupsIndex = eventParticipantGroupsIndex;

  $scope.getParticipantCount = function (participantGroupID) {
    return $scope.eventParticipantsIndex.filter(function (ep) {
      var pID = $scope.model.eventParticipants.hashMap[ep].participantID;
      var pgID = $scope.model.participants.hashMap[pID].participantGroupID;
      return pgID == participantGroupID;
    }).length;
  };

  $scope.$on("updateProgress", function (event, data) {
    $msUI.showToast(data.message, data.updateProgressType);
  });
});

