app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  .state('app.system.site-admin.career-sets', {
    url: '/career-sets',
    templateUrl: '/client/app/system/site-admin/career-sets/site-career-sets.html',
    controller: 'SiteCareerSetsController',
    data: {
      stateMapName: 'Career Sets',
      pageTitle: "Career Sets",
      backState: ""
    }
  })

  ;// closes $stateProvider
}]);
