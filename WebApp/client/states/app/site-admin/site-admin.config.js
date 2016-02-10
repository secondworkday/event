app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  // SITE ADMIN --------------------------------------------------------------- //

  .state('app.site-admin', {
    templateUrl: "/client/states/app/site-admin/site-admin.html",
    controller: 'SiteAdminController'
  })
  .state('app.site-admin.menu', {
    url: '/a/menu',
    templateUrl: "/client/states/app/site-admin/site-admin-menu.html",
    data: {
      pageTitle: "Menu",
      backState: ""
    }
  })
  .state('app.site-admin.system', {
    url: '/system',
    template: "<ui-view/>",
    resolve: {
      // We're a system admin - gloves off, remove the tenant lock
      tenant: function () { return null },
    },
    data: {
      stateMapName: 'System Admin',
      stateMapComment: 'check authorization',
      allowedRoles: [AUTHORIZATION_ROLES.systemAdmin]
    }
  })
  .state('app.site-admin.system.dashboard', {
    url: '/dashboard',
    templateUrl: '/client/states/app/site-admin/system-dashboard.html',
    controller: 'SystemDashboardController',
    data: {
      stateMapName: 'System Admin Dashboard',
      landingZone: true,
      pageTitle: "Dashboard",
      backState: ""
    }
  })
  .state('app.site-admin.system.users', {
    url: '/users',
    templateUrl: '/client/states/app/site-admin/system-users.html',
    controller: 'SystemUsersController',
    data: {
      stateMapName: 'System Users',
      pageTitle: "Users",
      backState: ""
    }
  })
  .state('app.site-admin.system.user', {
    abstract: true,
    url: '/users/:userID',
    template: "<ui-view/>",
    controller: 'TenantUserController',
    resolve: {
      user: function ($stateParams, utilityService) {
        // careful - might be an invalid or unauthorized ID
        var userID = $stateParams.userID;
        //!! should we use ensureXyz here - to rely on cached info if we've got it?
        return utilityService.ensureUser(userID);
      }
    },
    data: {
      stateMapName: 'User abstract',
      stateMapComment: 'abstract, resolve :userID'
    }
  })
  .state('app.site-admin.system.user.account', {
    url: '/account',
    templateUrl: '/client/states/app/tenant-admin/user/tenant-user-account.html',
    data: {
      stateMapName: 'Tenant User Account',
      pageTitle: "Back",
      backState: "app.site-admin.system.users"
    }
  })
  .state('app.site-admin.system.settings', {
    url: '/settings',
    templateUrl: '/client/states/app/site-admin/system-settings.html',
    controller: 'SystemSettingsController',
    data: {
      stateMapName: 'System Settings',
      pageTitle: "Settings",
      backState: ""
    }
  })
  .state('app.site-admin.system.status', {
    url: '/status',
    templateUrl: '/client/states/app/site-admin/system-status.html',
    controller: 'SystemStatusController',
    data: {
      stateMapName: 'System Status',
      pageTitle: "Status",
      backState: ""
    }
  })
  .state('app.site-admin.system.event-log', {
    url: '/log',
    templateUrl: '/client/states/app/site-admin/event-log.html',
    controller: 'EventLogController',
    data: {
      stateMapName: 'System Event Log',
      pageTitle: "Event Log",
      backState: ""
    }
  })
  .state('app.site-admin.system.tenants', {
    url: '/tenants',
    templateUrl: '/client/states/app/site-admin/system-tenants.html',
    controller: 'SystemTenantsController',
    data: {
      stateMapName: 'System Tenants',
      pageTitle: "Tenants",
      backState: ""
    }
  })
  .state('app.site-admin.system.tenant', {
    abstract: true,
    url: '/tenants/:tenantID',
    template: "<ui-view/>",
    controller: 'TenantController',
    resolve: {
      tenant: function ($stateParams, utilityService) {
        // careful - might be an invalid or unauthorized tenantID
        var tenantID = $stateParams.tenantID;
        //!! should we use ensureTenant here - to rely on cached info if we've got it?
        return utilityService.getTenant(tenantID);
      }
    },
    data: {
      stateMapName: 'Tenant abstract',
      stateMapComment: 'abstract, resolve :tenantID'
    }
  })
  .state('app.site-admin.system.tenant.account', {
    url: '/account',
    templateUrl: '/client/states/app/site-admin/system-tenant-account.html',
    controller: 'SystemTenantAccountController',
    data: {
      stateMapName: 'Tenant Account',
      pageTitle: "Back",
      backState: "app.site-admin.system.tenants"
    }
  })
  .state('app.site-admin.system.tenant.dashboard', {
    url: '/dashboard',
    templateUrl: '/client/states/app/admin/tenant-dashboard.html',
    controller: 'TenantDashboardController',
    data: {
      stateMapName: 'Tenant Dashboard (as System)',
      stateMapComment: 'change identity and redirect',
      landingZone: true,
      pageTitle: "Back",
      backState: "app.site-admin.system.tenants"
    }
  })
  .state('app.site-admin.system.tenant.groups', {
    url: '/groups',
    templateUrl: '/client/states/app/admin/tenant-groups.html',
    controller: 'TenantGroupsController',
    data: {
      stateMapName: 'Tenant Groups (as System)',
      stateMapComment: 'change identity and redirect',
      pageTitle: "Back",
      backState: "app.site-admin.system.tenants"
    }
  })
  .state('app.site-admin.system.tenant.history', {
    url: '/history',
    templateUrl: '/client/states/app/admin/tenant-history.html',
    controller: 'TenantHistoryController',
    data: {
      stateMapName: 'Tenant History (as System)',
      stateMapComment: 'change identity and redirect',
      pageTitle: "Back",
      backState: "app.site-admin.system.tenants"
    }
  })
  .state('app.site-admin.system.tenant.settings', {
    url: '/settings',
    templateUrl: '/client/states/app/admin/tenant-settings.html',
    controller: 'TenantSettingsController',
    data: {
      stateMapName: 'Tenant Settings (as System)',
      stateMapComment: 'change identity and redirect',
      pageTitle: "Back",
      backState: "app.site-admin.system.tenants"
    }
  })
  .state('app.site-admin.system.tenant.users', {
    url: '/users',
    templateUrl: '/client/states/app/admin/tenant-users.html',
    controller: 'TenantUsersController',
    data: {
      stateMapName: 'Tenant Users (as System)',
      stateMapComment: 'change identity and redirect',
      pageTitle: "Back",
      backState: "app.site-admin.system.tenants"
    }
  })
  .state('app.site-admin.ops', {
    url: '/ops',
    template: "<ui-view/>",
    resolve: {
      // TODO check authorization for System Admin
    }
  })
  .state('app.site-admin.ops.dashboard', {
    url: '/dashboard',
    templateUrl: '/client/states/app/site-admin/ops-dashboard.html',
    controller: 'OpsDashboardController',
    data: {
      stateMapName: 'Ops Dashboard',
      pageTitle: "Ops Dashboard",
      backState: ""
    }
  })
  .state('app.site-admin.ops.log', {
    url: '/log',
    templateUrl: '/client/states/app/site-admin/event-log.html',
    controller: 'EventLogController',
    data: {
      stateMapName: 'Ops Event Log',
      pageTitle: "Ops Log",
      backState: ""
    }
  })
  .state('app.site-admin.ops.diagnostics', {
    url: '/diagnostics',
    templateUrl: '/client/states/app/site-admin/ops-diagnostics.html',
    controller: 'OpsDiagnosticsController',
    data: {
      stateMapName: 'Ops Diagnostics',
      pageTitle: "Diagnostics",
      backState: ""
    }
  })
  .state('app.site-admin.ops.backup', {
    url: '/backup',
    templateUrl: '/client/states/app/site-admin/ops-backup.html',
    controller: 'OpsBackupController',
    data: {
      stateMapName: 'Ops Backup',
      pageTitle: "Backup",
      backState: ""
    }
  })
  .state('app.site-admin.database', {
    url: '/database',
    template: "<ui-view/>",
    resolve: {
      // TODO check authorization for System Admin
    }
  })
  .state('app.site-admin.database.dashboard', {
    url: '/dashboard',
    templateUrl: '/client/states/app/site-admin/database-dashboard.html',
    controller: 'DatabaseDashboardController',
    data: {
      stateMapName: 'Database Dashboard',
      pageTitle: "DB Dashboard",
      backState: ""
    }
  })
  .state('app.site-admin.database.data-import', {
    url: '/data-import',
    templateUrl: '/client/states/app/site-admin/database-data-import.html',
    controller: 'DatabaseDataImportController',
    data: {
      stateMapName: 'Database Data Import',
      pageTitle: "Data Import",
      backState: ""
    }
  })
  .state('app.site-admin.database.data-export', {
    url: '/data-export',
    templateUrl: '/client/states/app/site-admin/database-data-export.html',
    controller: 'DatabaseDataExportController',
    data: {
      stateMapName: 'Database Data Export',
      pageTitle: "Data Export",
      backState: ""
    }
  })
  .state('app.site-admin.database.log', {
    url: '/log',
    templateUrl: '/client/states/app/site-admin/database-log.html',
    controller: 'DatabaseLogController',
    data: {
      stateMapName: 'Database Log',
      pageTitle: "Log",
      backState: ""
    }
  })

  ;// closes $stateProvider
}]);
