app.controller('EventController', function ($scope, $log, $state, $mdDialog, $msUI, utilityService, siteService, event) {

  $scope.event = event;

  $scope.$on("updateProgress", function (event, data) {
    $msUI.showToast(data.message, data.updateProgressType);
  });

  // It's our responsibility to pre-load all the EventParticipants in scope
  siteService.model.eventParticipants.search("$event:" + event.id, "", 0, 99999);
});