app.controller('SystemLibraryOccupationsController', function ($scope, $log, $mdDialog, utilityService, siteService) {
  $log.debug("Loading SystemLibraryOccupationsController...");

  $scope.sortOptions = [
    { name: 'Title', serverTerm: 'occupation.OccupationTitle', clientFunction: utilityService.localeCompareByPropertyThenByID("displayTitle") },
    { name: 'Title Descending', serverTerm: 'occupation.OccupationTitle DESC', clientFunction: utilityService.localeCompareByPropertyThenByIDDescending("displayTitle") },
    { name: 'O*Net Code', serverTerm: 'occupation.OnetOccupation.OnetCode', clientFunction: utilityService.compareByPropertyThenByID("id") },
    { name: 'O*Net Code Descending', serverTerm: 'occupation.OnetOccupation.OnetCode DESC', clientFunction: utilityService.compareByPropertyThenByIDDescending("id") },
    { name: 'Employment Descending', serverTerm: 'lmai.TotalEmployment DESC', clientFunction: utilityService.compareByPropertyThenByIDDescending("totalEmployment") }
  ];

  var filterByReleaseStateFactory = function (includeReleaseState) {
    var includeReleaseStateLocal = includeReleaseState;
    return function (item) {
      return item.releaseState === includeReleaseStateLocal;
    };
  };

  $scope.filterOptions = [
    { name: 'All' },
    { name: 'No Image', serverTerm: '$NoImage', clientFunction: filterByReleaseStateFactory("Red") },
    { name: 'Image', serverTerm: '$Image', clientFunction: filterByReleaseStateFactory("Red") },
    { name: 'Red', serverTerm: '$Red', clientFunction: filterByReleaseStateFactory("Red") },
    { name: 'Yellow', serverTerm: '$Yellow', clientFunction: filterByReleaseStateFactory("Yellow") },
    { name: 'Green', serverTerm: '$Green', clientFunction: filterByReleaseStateFactory("Green") }
  ];

  $scope.searchViewOptions = {
    sort: $scope.sortOptions[0],
    filter: $scope.filterOptions[0]
  };


  $scope.searchOccupations = siteService.model.occupations.search;


  $scope.showLibraryBatchUpdateDialog = function ($event) {
     var parentEl = angular.element(document.body);
     $mdDialog.show({
       parent: parentEl,
       targetEvent: $event,
       templateUrl: '/client/states/app/site-admin/system-library-batch-update.dialog.html',
       controller: LibraryBatchUpdateDialogController
    });
    function LibraryBatchUpdateDialogController($scope, $mdDialog) {
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

});
