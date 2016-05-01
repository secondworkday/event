app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  .state('app.system.site-admin', {
    url: '/site',
    templateUrl: "/client/app/system/site-admin/site-admin.html",
    controller: 'SiteAdminController',
    resolve: {
      // We're a system admin - gloves off, remove the tenant lock
      tenant: function () { return null; },
    },
    data: {
      allowedRoles: [AUTHORIZATION_ROLES.systemAdmin]
    }
  })
  .state('app.system.site-admin.menu', {
    url: '/menu',
    templateUrl: "/client/app/system/site-admin/site-admin-menu.html",
    data: {
      pageTitle: "Menu",
      backState: ""
    }
  })
  .state('app.system.site-admin.dashboard', {
    url: '/dashboard',
    templateUrl: '/client/app/system/site-admin/site-admin-dashboard.html',
    controller: 'SiteAdminDashboardController',
    data: {
      pageTitle: "Site Dashboard",
      backState: ""
    }
  })
  .state('app.system.site-admin.site-settings', {
    url: '/settings',
    templateUrl: '/client/app/system/site-admin/site-settings.html',
    controller: 'SiteSettingsController',
    data: {
      pageTitle: "Site Settings",
      backState: ""
    }
  })
  .state('app.system.site-admin.users', {
    url: '/users',
    templateUrl: '/client/app/system/site-admin/site-users.html',
    controller: 'SiteUsersController',
    data: {
      pageTitle: "Site Users",
      backState: ""
    }
  })
  .state('app.system.site-admin.user-settings', {
    url: '/users/:userID',
    templateUrl: "/client/app/system/site-admin/user-settings.html",
    controller: 'UserSettingsController',
    resolve: {
      user: function ($stateParams, utilityService) {
        // careful - might be an invalid or unauthorized ID
        var userID = $stateParams.userID;
        //!! should we use ensureXyz here - to rely on cached info if we've got it?
        return utilityService.users.ensure(userID);
      }
    },
    data: {
      pageTitle: "Back",
      backState: "app.system.site-admin.users"
    }
  })
  .state('app.system.site-admin.site-status', {
    url: '/site-status',
    templateUrl: '/client/app/system/site-admin/site-status.html',
    controller: 'SiteStatusController',
    data: {
      pageTitle: "Site Status",
      backState: ""
    }
  })
  .state('app.system.site-admin.activity-log', {
    url: '/activity-log',
    templateUrl: '/client/app/system/site-admin/site-activity-log.html',
    controller: 'SiteActivityLogController',
    data: {
      pageTitle: "Activity Log",
      backState: ""
    }
  })
  .state('app.system.site-admin.event-log', {
    url: '/event-log',
    templateUrl: '/client/app/system/ops-admin/event-log.html',
    controller: 'EventLogController',
    data: {
      pageTitle: "Event Log",
      backState: ""
    }
  })
  .state('app.system.site-admin.tenants', {
    url: '/tenants',
    templateUrl: '/client/app/system/site-admin/site-tenants.html',
    controller: 'SiteTenantsController',
    data: {
      pageTitle: "Tenants",
      backState: ""
    }
  })
  .state('app.system.site-admin.tenant-settings', {
    url: '/tenants/:tenantID',
    templateUrl: "/client/app/system/site-admin/tenant-settings.html",
    controller: 'TenantSettingsController',
    resolve: {
      tenant: function ($stateParams, utilityService) {
        // careful - might be an invalid or unauthorized tenantID
        var tenantID = $stateParams.tenantID;
        //!! should we use ensureTenant here - to rely on cached info if we've got it?
        return utilityService.tenantGroups.ensure(tenantID);
      }
    },
    data: {
      pageTitle: "Back",
      backState: "app.system.site-admin.tenants"
    }
  })

  ;// closes $stateProvider
}]);
