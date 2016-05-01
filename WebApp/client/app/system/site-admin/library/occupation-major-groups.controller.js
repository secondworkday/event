(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('OccupationMajorGroupsController', OccupationMajorGroupsController);

    OccupationMajorGroupsController.$inject = ['$scope', '$log', 'siteService'];

    /* @ngInject */
    function OccupationMajorGroupsController($scope, $log, siteService) {
      $log.debug("Loading SystemLibraryOccupationMajorGroupsController...");

      $scope.occupationMajorGroups = $scope.model.occupationMajorGroups;

      // Since we have a fixed set of Occupation Major Groups, we need to query the server to freshen
      // those items with their current state.
      angular.forEach($scope.occupationMajorGroups, function (occupationMajorGroup) {
        // we grab them one at a time, not particularly efficient
        siteService.getOccupationAuxiliaryData(occupationMajorGroup.onetCode);
      });
    }
})();
