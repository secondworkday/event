app.controller('EventSessionController', function ($scope, $translate, $log, $state, $mdDialog, $msUI, utilityService, siteService, event, eventSession) {
  $log.debug('Loading EventSessionController...');
  $log.debug('Loading ' + $state.name + '...');

  $scope.event = event;
  $scope.eventSession = eventSession;

  //!! TODO: replace these hard coded values with real server-provided values
  $scope.eventSession.checkedInParticipantsKThrough5 = 111;
  $scope.eventSession.totalParticipantsKThrough5 = 999;
  $scope.eventSession.checkedInParticipants6Through12 = 222;
  $scope.eventSession.totalParticipants6Through12 = 888;



  // ** Create some EventParticipant indexers - so we can track our check-in & check-out progress through the EventSession

  var indexerFactory = function (selectFilterExpression) {
    var indexer = {
      index: [],
      sort: utilityService.compareByProperties('id'),
      baseFilter: function (item) {
        return item.eventSessionID === $scope.eventSession.id;
      }
    };
    if (selectFilterExpression) {
      indexer.selectFilter =  Function("item", "return " + selectFilterExpression);
    }
    return indexer;
  }

  eventSession.allParticipants = indexerFactory();
  eventSession.expectedParticipants = indexerFactory('!item.checkInTimestamp');
  eventSession.checkedInParticipants = indexerFactory('item.checkInTimestamp && !item.checkOutTimestamp');
  eventSession.checkedOutParticipants = indexerFactory('item.checkInTimestamp && item.checkOutTimestamp');

  var lowLevelExpression = '(item.level && (isNaN(item.level) || item.level <= 5))'
  var highLevelExpression = '(item.level && !isNaN(item.level) && item.level > 5)'
  eventSession.noLevelParticipants = indexerFactory('!item.level');
  eventSession.noLevelCheckedInParticipants = indexerFactory('!item.level && item.checkInTimestamp && !item.checkOutTimestamp');
  eventSession.lowLevelParticipants = indexerFactory(lowLevelExpression);
  eventSession.lowLevelCheckedInParticipants = indexerFactory(lowLevelExpression + ' && item.checkInTimestamp && !item.checkOutTimestamp');
  eventSession.highLevelParticipants = indexerFactory(highLevelExpression);
  eventSession.highLevelCheckedInParticipants = indexerFactory( highLevelExpression + ' && item.checkInTimestamp && !item.checkOutTimestamp');


  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.allParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.expectedParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.checkedInParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.checkedOutParticipants);

  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.noLevelParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.noLevelCheckedInParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.lowLevelParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.lowLevelCheckedInParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.highLevelParticipants);
  utilityService.registerIndexer($scope.model.eventParticipants, eventSession.highLevelCheckedInParticipants);

  $scope.$on("$destroy", function () {
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.allParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.expectedParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.checkedInParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.checkedOutParticipants);

    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.noLevelParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.noLevelCheckedInParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.lowLevelParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.lowLevelCheckedInParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.highLevelParticipants);
    utilityService.unRegisterIndexer($scope.model.eventParticipants, eventSession.highLevelCheckedInParticipants);
  });



  // It's our responsibility to pre-load all the EventParticipants in scope
  siteService.model.eventParticipants.search("$eventSession:" + eventSession.id, "", 0, 99999);
});
