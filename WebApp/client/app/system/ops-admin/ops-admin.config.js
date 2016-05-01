app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  .state('app.system.ops-admin', {
    url: '/ops',
    template: "<ui-view/>",
    resolve: {
      // TODO check authorization for System Admin
    }
  })
  .state('app.system.ops-admin.dashboard', {
    url: '/dashboard',
    templateUrl: '/client/app/system/ops-admin/ops-dashboard.html',
    controller: 'OpsDashboardController',
    data: {
      stateMapName: 'Ops Dashboard',
      pageTitle: "Ops Dashboard",
      backState: ""
    }
  })
  .state('app.system.ops-admin.log', {
    url: '/log',
    templateUrl: '/client/app/system/ops-admin/event-log.html',
    controller: 'EventLogController',
    data: {
      stateMapName: 'Ops Event Log',
      pageTitle: "Ops Log",
      backState: ""
    }
  })
  .state('app.system.ops-admin.diagnostics', {
    url: '/diagnostics',
    templateUrl: '/client/app/system/ops-admin/ops-diagnostics.html',
    controller: 'OpsDiagnosticsController',
    data: {
      stateMapName: 'Ops Diagnostics',
      pageTitle: "Diagnostics",
      backState: ""
    }
  })
  .state('app.system.ops-admin.backup', {
    url: '/backup',
    templateUrl: '/client/app/system/ops-admin/ops-backup.html',
    controller: 'OpsBackupController',
    data: {
      stateMapName: 'Ops Backup',
      pageTitle: "Backup",
      backState: ""
    }
  })

  ;// closes $stateProvider
}]);
