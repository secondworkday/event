app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider
  .state('app.system.site-admin.library', {
    url: '/library',
    templateUrl: '/client/app/system/site-admin/library/library.html',
    controller: 'LibraryController',
    data: {
      stateMapName: 'Library',
      pageTitle: "Library",
      backState: ""
    }
  })
  .state('app.system.site-admin.library.occupations', {
    url: '/occupations',
    data: {
      'selectedTab': 0,
      pageTitle: "Library",
      backState: ""
    },
    views: {
      'occupations': {
        templateUrl: '/client/app/system/site-admin/library/occupations.html',
        controller: "OccupationsController"
      }
    }
  })
  .state('app.system.site-admin.library.occupations.occupation', {
    url: '/:occupationCode',
    templateUrl: "/client/app/system/site-admin/library/occupation-details.html",
    controller: "OccupationDetailsController",
    resolve: {
      occupation: function ($stateParams, siteService) {
        var occupationCode = $stateParams.occupationCode;
        return siteService.occupations.ensure(occupationCode);
      }
    }
  })
  .state('app.system.site-admin.library.occupation-major-groups', {
    url: '/major-groups',
    data: {
      'selectedTab': 1,
      pageTitle: "Library",
      backState: ""
    },
    views: {
      'occupation-major-groups': {
        templateUrl: '/client/app/system/site-admin/library/occupation-major-groups.html',
        controller: "OccupationMajorGroupsController"
      }
    }
  })
  .state('app.system.site-admin.library.occupation-major-groups.occupation-major-group', {
    url: '/:occupationMajorGroupCode',
    templateUrl: "/client/app/system/site-admin/library/occupation-major-group-details.html",
    controller: "OccupationMajorGroupDetailsController",
    resolve: {
      occupationMajorGroup: function ($stateParams, siteService) {
        var occupationMajorGroupCode = $stateParams.occupationMajorGroupCode;
        return siteService.getOccupationAuxiliaryData(occupationMajorGroupCode);
      }
    }
  })

  ;// closes $stateProvider
}]);
