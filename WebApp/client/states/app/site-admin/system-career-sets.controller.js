app.controller('SystemCareerSetsController', function ($scope, $log, $mdDialog, utilityService, siteService) {
  $log.debug('Loading SystemCareerSetsController...');

  $scope.filters = [
    {name: 'Ex Offender Jobs', createdBy: 'Bill Johnston', dateCreated: '3 days ago', isHidden: false, isShared: true},
    {name: 'hair', createdBy: 'Cheryl Boebel', dateCreated: '4 days ago', isHidden: false, isShared: true},
    {name: '1st line supervisors', createdBy: 'Helena Bucca', dateCreated: '6 days ago', isHidden: false, isShared: true},
    {name: 'AEC Green', createdBy: 'Stephen Judy', dateCreated: '7 days ago', isHidden: true, isShared: false},
    {name: 'Casino Prospects', createdBy: 'Matthew Burke', dateCreated: '12 days ago', isHidden: true, isShared: false}
  ];

  $scope.showNewSystemFilterDialog = function ($event) {
     var parentEl = angular.element(document.body);
     $mdDialog.show({
       parent: parentEl,
       targetEvent: $event,
       templateUrl: '/client/states/app/site-admin/system-new-filter.dialog.html',
       locals: {
         jobFamilies: $scope.jobFamilies,
         industries: $scope.industries
       },
       controller: NewSystemFilterDialogController
    });
    function NewSystemFilterDialogController($scope, $mdDialog, jobFamilies, industries) {
      $scope.jobFamilies = jobFamilies;
      $scope.industries = industries;
      $scope.createFilter = function(formData) {
        // TODO add create filter code
        $mdDialog.hide();
      };
      $scope.cancelDialog = function() {
        $log.debug( "You canceled the dialog." );
        $mdDialog.hide();
      };
    }
  };

});
