app.controller('ActivityLogController', function ($scope, $state, $log, $q, utilityService, siteService) {
  $log.debug('Loading ActivityLogController...');


  // UtilityService doesn't track ActivityItems, so we have to setup a cache and manage that ourselves
  $scope.activityLog = utilityService.createItemCache(utilityService.searchActivityLog);

  $scope.demandUser = utilityService.demandUser;
  $scope.model = utilityService.model;

  $scope.searchActivityLog = function (searchExpression, sortExpression, startIndex, rowCount) {
    return $scope.activityLog.search(searchExpression, sortExpression, startIndex, rowCount)
    .then(function (itemsData) {
      return expandItems(itemsData.newIDs)
      .then(function () {
        // (caller still expects to see the original itemsData)
        return itemsData;
      });
    });
  };

  function expandItems(itemIDs) {
    var model = utilityService.model;

    var promises = [];

    angular.forEach(itemIDs, function (itemID) {
      var item = $scope.activityLog.hashMap[itemID];
      if (item.createdByUserID) {
        //!!promises.push(utilityService.
        item.user = utilityService.demandUser(item.createdByUserID);
      }

      if (item.targetTable === "EventParticipant") {
        item.eventParticipantID = item.targetID;
        promises.push(siteService.eventParticipants.ensure(item.eventParticipantID)
          .then(function (eventParticipant) {
            item.eventID = eventParticipant.eventID;
            item.eventSessionID = eventParticipant.eventSessionID;
            return eventParticipant;
          }));
      }

      if (item.contextTable === "Event") {
        item.eventID = item.contextID;
      }
    });

    return $q.all(promises);
  }

  $scope.$on('updateActivityLog', function (event, itemsData) {
    console.log("hey");

    var notification = utilityService.updateItemsModel($scope.activityLog, itemsData);

    expandItems(notification.newIDs)
    .then(function () {
      // We have to delay sending out this notification until the item is expanded as we might need that expanded info for sorting/filtering
      $scope.$broadcast('activityLogUpdated', notification);
    });
  });



  $scope.searchViewOptions = {};

  // Establish our Base filtering (evaluatuating in order of most restrictive to least restrictive)
  if ($scope.eventSession) {
    // filter to one EventSession
    $scope.searchViewOptions.baseFilter = { serverTerm: '$eventSession:' + $scope.eventSession.id, clientFunction: utilityService.filterByPropertyValue('eventSessionID', $scope.eventSession.id) };
  } else if ($scope.event) {
    // filter to one Event
    $scope.searchViewOptions.baseFilter = { serverTerm: '$event:' + $scope.event.id, clientFunction: utilityService.filterByPropertyValue('eventID', $scope.event.id) };
  } else {
    // no filtering
  }


  $scope.sortOptions = [
    { name: 'All', serverTerm: '', clientFunction: utilityService.compareByProperties('id') },

    { name: 'Most Recent Descending', serverTerm: 'CreatedTimestamp DESC', clientFunction: utilityService.compareByProperties('-createdTimestamp', '-id') },
    { name: 'Most Recent Descending', serverTerm: 'CreatedTimestamp DESC', clientFunction: utilityService.compareByProperties('-createdTimestamp', '-id') }



    //{ name: 'Last Name', serverTerm: 'Participant.LastName', clientFunction: utilityService.compareByProperties('lastName', 'id') },
    //!! this is currently broken - as we don't really want to sort by the ParticipantGroup ID
    //{ name: 'School', serverTerm: 'Participant.ParticipantGroup.Name', clientFunction: utilityService.compareByProperties('participantGroupName', 'id') },
    //{ name: 'Grade', serverTerm: 'ExEventParticipant.item.Grade', clientFunction: utilityService.compareByProperties('grade', 'id') }
  ];

  $scope.searchViewOptions.sort = $scope.sortOptions[1];

  $scope.filterOptions = [
    //{ name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
    //{ name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },
    { name: 'All' }
  ];


  $scope.showActivityLogSelection = function (activity) {
    $state.go('app.user.event.participants', { eventID: activity.contextID, activityID: activity.id });
  };




  $scope.$on("$destroy", function () {
    // unrequest server notifications
    utilityService.untrackActivityLog();
  });

  // init

  // request server notifications
  utilityService.trackActivityLog();


});
