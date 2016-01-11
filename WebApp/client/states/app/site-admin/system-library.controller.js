app.controller('SystemLibraryController', function ($scope, $log, $state, FileUploader, utilityService, siteService) {
  $log.debug("Loading SystemLibraryController...");

  // $scope.$on('$stateChangeSuccess', function(event, toState, toParams, fromState, fromParams) {
  //   $scope.currentTab = toState.data.selectedTab;
  // });

  $scope.currentState = $state.current;

  $scope.imageSources = ['iStockPhoto', 'Flickr', 'Getty Images', 'Shutterstock', 'Adobe', 'Veer', 'Big Stock Photo', 'Corbis', 'Fotolia', 'Little Visuals', 'Unsplash', 'Superfamous', 'Picjumbo', 'Gratisography', 'Public Domain Archive', 'Other'];

  $scope.licenseTypes = ['No license requirements', 'License requirements not fulfilled', 'License requirements fulfilled' ];

});
