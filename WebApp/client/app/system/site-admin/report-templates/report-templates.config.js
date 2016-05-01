app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  .state('app.system.site-admin.report-templates', {
    url: '/report-templates',
    templateUrl: '/client/app/system/site-admin/report-templates/report-templates.html',
    controller: 'ReportTemplatesController',
    data: {
      stateMapName: 'Report Templates',
      pageTitle: "Report Templates",
      backState: ""
    }
  })

  ;// closes $stateProvider
}]);
