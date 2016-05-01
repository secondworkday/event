app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  .state('app.system.database-admin', {
    url: '/database',
    template: "<ui-view/>",
    resolve: {
      // TODO check authorization for System Admin
    }
  })
  .state('app.system.database-admin.dashboard', {
    url: '/dashboard',
    templateUrl: '/client/app/system/database-admin/database-dashboard.html',
    controller: 'DatabaseDashboardController',
    data: {
      stateMapName: 'Database Dashboard',
      pageTitle: "DB Dashboard",
      backState: ""
    }
  })
  .state('app.system.database-admin.data-import', {
    url: '/data-import',
    templateUrl: '/client/app/system/database-admin/database-data-import.html',
    controller: 'DatabaseDataImportController',
    data: {
      stateMapName: 'Database Data Import',
      pageTitle: "Data Import",
      backState: ""
    }
  })
  .state('app.system.database-admin.data-export', {
    url: '/data-export',
    templateUrl: '/client/app/system/database-admin/database-data-export.html',
    controller: 'DatabaseDataExportController',
    data: {
      stateMapName: 'Database Data Export',
      pageTitle: "Data Export",
      backState: ""
    }
  })
  .state('app.system.database-admin.log', {
    url: '/log',
    templateUrl: '/client/app/system/database-admin/database-log.html',
    controller: 'DatabaseLogController',
    data: {
      stateMapName: 'Database Log',
      pageTitle: "Log",
      backState: ""
    }
  })

  ;// closes $stateProvider
}]);
