(function () {
  'use strict';

  angular
      .module('myApp')
      .controller('TenantSettingsController', TenantSettingsController);

  TenantSettingsController.$inject = ['$scope', '$log', '$q', 'utilityService', 'siteService', 'tenant'];

  /* @ngInject */
  function TenantSettingsController($scope, $log, $q, utilityService, siteService, tenant) {
    $log.debug("Loading TenantSettingsController...");

    $scope.tenant = tenant;

    $scope.model = utilityService.model;
    $scope.demandUser = utilityService.users.demand;


    //!! TODO currently not displaying the correct activity log (the one for $scope.tenant)

    // UtilityService doesn't track ActivityItems, so we have to setup a cache and manage that ourselves
    $scope.activityLog = utilityService.createItemCache(siteService.searchActivityLog);

    $scope.settings = [
      { name: 'Allow tenant to send emails', enabled: true },
      { name: 'Allow create new groups', enabled: true },
      { name: 'Allow create new users', enabled: true }
    ];

    $scope.setTenantGroupAccountOption = function (name, value) {
      utilityService.modifyTenantGroupAccountOption($scope.tenant, name, value)
      .catch(function (failureData) {
        $log.warn(failureData.errorMessage);
      });
    };

    $scope.setPublicTenantGroupAccountOption = function (name, value) {
      utilityService.modifyTenantGroupAccountOption($scope.tenant, 'isPublic', value)
      .then(function () {
        return utilityService.modifyTenantGroupAccountOption($scope.tenant, name, value);
      })
      .catch(function (failureData) {
        $log.warn(failureData.errorMessage);
      });
    };



    $scope.setDemoTenant = function (isDemoTenant) {
      utilityService.modifyTenantGroupAccountOption($scope.tenant, 'isDemo', isDemoTenant)
          .then(function (successData) {
          }, function (failureData) {
            alert(failureData.errorMessage);
          });
    };

    $scope.toggleSetting = function (setting) {
      switch (setting.name) {
        case 'Demo tenant':
          $log.error("Implement me 22", setting);
          break;
        default:
          $log.error("Implement me", setting);
          break;
      }
    };

    $scope.$on('updateActivityLog', function (event, itemsData) {
      console.log("hey");
      siteService.unpackActivityLogItems(itemsData)
      .then(function () {
        // We have to delay sending out this notification until the item is expanded as we might need that expanded info for sorting/filtering
        var notification = utilityService.updateItemsModel($scope.activityLog, itemsData);
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


    $scope.$on("$destroy", function () {
      // unrequest server notifications
      utilityService.untrackActivityLog();
    });

    // init
    utilityService.trackActivityLog();

  }
})();
