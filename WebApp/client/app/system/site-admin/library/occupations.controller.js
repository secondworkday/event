(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('OccupationsController', OccupationsController);

    OccupationsController.$inject = ['$scope', '$log', '$mdDialog', 'utilityService', 'siteService'];

    /* @ngInject */
    function OccupationsController($scope, $log, $mdDialog, utilityService, siteService) {
      $log.debug("Loading OccupationsController...");

      $scope.searchOccupations = siteService.model.occupations.search;

      $scope.sortOptions = [
        { name: 'Title', serverTerm: 'occupation.OccupationTitle', clientFunction: utilityService.compareByProperties('displayTitle', 'id') },
        { name: 'Title Descending', serverTerm: 'occupation.OccupationTitle DESC', clientFunction: utilityService.compareByProperties('-displayTitle', '-id') },
        { name: 'O*Net Code', serverTerm: 'occupation.OnetOccupation.OnetCode', clientFunction: utilityService.compareByProperties('id') },
        { name: 'O*Net Code Descending', serverTerm: 'occupation.OnetOccupation.OnetCode DESC', clientFunction: utilityService.compareByProperties('-id') },
        { name: 'Employment Descending', serverTerm: 'lmai.TotalEmployment DESC', clientFunction: utilityService.compareByProperties('-totalEmployment', '-id') }
      ];

      $scope.filterOptions = [
        { name: 'All' },
        //!! TODO add inheritedImage as a filter option
        { name: 'No Image', serverTerm: '$NoImage', clientFunction: utilityService.filterByPropertyHasValue("!heroImageUrl") },
        { name: 'Image', serverTerm: '$Image', clientFunction: utilityService.filterByPropertyHasValue("heroImageUrl") },
        { name: 'Red', serverTerm: '$Red', clientFunction: utilityService.filterByPropertyValue("releaseState", "Red") },
        { name: 'Yellow', serverTerm: '$Yellow', clientFunction: utilityService.filterByPropertyValue("releaseState", "Yellow") },
        { name: 'Green', serverTerm: '$Green', clientFunction: utilityService.filterByPropertyValue("releaseState", "Green") }
      ];

      $scope.searchViewOptions = {
        sort: $scope.sortOptions[0],
        filter: $scope.filterOptions[0],
        userSearch: null
      };


      $scope.showOccupationsBatchUpdateDialog = function ($event) {
         var parentEl = angular.element(document.body);
         $mdDialog.show({
           parent: parentEl,
           targetEvent: $event,
           templateUrl: '/client/app/system/ms-site-admin/occupations-batch-update.dialog.html',
           controller: OccupationsBatchUpdateDialogController
        });
        function OccupationsBatchUpdateDialogController($scope, $mdDialog) {
          $scope.downloadTemplate = function() {
            $log.debug("Download template file.");
          };
          $scope.upload = function() {
            $log.debug("Upload edited file.");
          };
          $scope.applyEdits = function() {
            $log.debug("Apply edits and close dialog.");
            $mdDialog.hide();
          };
          $scope.discardEdits = function() {
            $log.debug("Discard edits and close dialog.");
            $mdDialog.hide();
          };
          $scope.cancelDialog = function() {
            $log.debug("Cancel dialog.");
            $mdDialog.hide();
          };
        }
      };
    }
})();
