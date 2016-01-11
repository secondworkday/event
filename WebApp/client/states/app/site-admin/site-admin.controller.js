app.controller('SiteAdminController', function ($scope, $rootScope, $state, $log, siteService) {
  $log.debug('Loading SiteAdminController...');

  $scope.stateData = $state.current.data;

  $scope.systemAdminPages = [
    { pageTitle: "Dashboard", icon: "fa fa-table", uiSref: "app.site-admin.system.dashboard", active: false},
    { pageTitle: "Tenants", icon: "fa fa-building-o", uiSref: "app.site-admin.system.tenants", active: false},
    { pageTitle: "Users", icon: "fa fa-users", uiSref: "app.site-admin.system.users", active: false},
    { pageTitle: "Library", icon: "fa fa-book", uiSref: "app.site-admin.system.library.occupations", active: false},
    { pageTitle: "Career Sets", icon: "fa fa-th-large", uiSref: "app.site-admin.system.career-sets", active: false},
    { pageTitle: "Settings", icon: "fa fa-sliders", uiSref: "app.site-admin.system.settings", active: false},
    { pageTitle: "Status", icon: "fa fa-power-off", uiSref: "app.site-admin.system.status", active: false},
    { pageTitle: "Event Log", icon: "fa fa-list-ul", uiSref: "app.site-admin.system.event-log", active: false}
  ];

  $scope.opsAdminPages = [
    { pageTitle: "Ops Dashboard", icon: "fa fa-table", uiSref: "app.site-admin.ops.dashboard", active: false},
    { pageTitle: "Ops Log", icon: "fa fa-list-ul", uiSref: "app.site-admin.ops.log", active: false},
    { pageTitle: "Diagnostics", icon: "fa fa-exclamation-triangle", uiSref: "app.site-admin.ops.diagnostics", active: false},
    { pageTitle: "Backup", icon: "fa fa-upload", uiSref: "app.site-admin.ops.backup", active: false}
  ];

  $scope.databaseAdminPages = [
    { pageTitle: "DB Dashboard", icon: "fa fa-table", uiSref: "app.site-admin.database.dashboard", active: false},
    { pageTitle: "Data Import", icon: "fa fa-upload", uiSref: "app.site-admin.database.data-import", active: false},
    { pageTitle: "Data Export", icon: "fa fa-download", uiSref: "app.site-admin.database.data-export", active: false},
    { pageTitle: "DB Log", icon: "fa fa-table", uiSref: "app.site-admin.database.log", active: false},
  ];



  $scope.subPages = [
    { pageTitle: "Help", icon: "fa fa-question-circle", uiSref: "app.site-admin.help", active: false}
  ];

  $rootScope.$on('$stateChangeSuccess', function (ev, to, toParams, from, fromParams) {
    $scope.stateData = $state.current.data;
    setActiveNavItem();
  });


  $scope.goBack = function(){
    $state.go($scope.stateData.backState);
  };

  function setActiveNavItem() {

    for (var i = 0; i < $scope.systemAdminPages.length; i++) {
      if ($scope.systemAdminPages[i].pageTitle == $scope.stateData.pageTitle) {
        $scope.systemAdminPages[i].active = true;
      } else {
        $scope.systemAdminPages[i].active = false;
      }
    }

    for (var i = 0; i < $scope.opsAdminPages.length; i++) {
      if ($scope.opsAdminPages[i].pageTitle == $scope.stateData.pageTitle) {
        $scope.opsAdminPages[i].active = true;
      } else {
        $scope.opsAdminPages[i].active = false;
      }
    }

    for (var i = 0; i < $scope.databaseAdminPages.length; i++) {
      if ($scope.databaseAdminPages[i].pageTitle == $scope.stateData.pageTitle) {
        $scope.databaseAdminPages[i].active = true;
      } else {
        $scope.databaseAdminPages[i].active = false;
      }
    }

    for (var i = 0; i < $scope.subPages.length; i++) {
      if ($scope.subPages[i].pageTitle == $scope.stateData.pageTitle) {
        $scope.subPages[i].active = true;
      } else {
        $scope.subPages[i].active = false;
      }
    }
  }

  setActiveNavItem();











  // $scope.systemSections = [
  //   {displayTitle: "Dashboard", uiSref: "app.site-admin.system.dashboard"},
  //   {displayTitle: "Tenants", uiSref: "app.site-admin.system.tenants"},
  //   {displayTitle: "Users", uiSref: "app.site-admin.system.users"},
  //   {displayTitle: "Filters", uiSref: "app.site-admin.system.filters"},
  //   {displayTitle: "Library", uiSref: "app.site-admin.system.library.occupations"},
  //   {displayTitle: "Status", uiSref: "app.site-admin.system.status"},
  //   {displayTitle: "Event Log", uiSref: "app.site-admin.system.eventlog"},
  //   {displayTitle: "Settings", uiSref: "app.site-admin.system.settings"}
  // ];
  //
  // $scope.opsSections = [
  //   {displayTitle: "Dashboard", uiSref: "app.site-admin.ops.dashboard"},
  //   {displayTitle: "Diagnostics", uiSref: "app.site-admin.ops.diagnostics"},
  //   {displayTitle: "Event Log", uiSref: "app.site-admin.ops.eventlog"},
  //   {displayTitle: "Backup", uiSref: "app.site-admin.ops.backup"}
  // ];
  //
  // $scope.databaseSections = [
  //   {displayTitle: "Dashboard", uiSref: "app.site-admin.database.dashboard"},
  //   {displayTitle: "Data Import", uiSref: "app.site-admin.database.data-import"},
  //   {displayTitle: "Data Export", uiSref: "app.site-admin.database.data-export"},
  //   {displayTitle: "Log", uiSref: "app.site-admin.database.log"}
  // ];

  // $scope.activeSection = null;
  //
  // for (var i = 0; i < $scope.systemSections.length; i++) {
  //   if ($state.includes($scope.systemSections[i].uiSref)) {
  //     $scope.activeSection = $scope.systemSections[i];
  //   } else {
  //     $log.debug("systemSections[" + i + "] is not the current state.");
  //   }
  // }
  //
  // for (var i = 0; i < $scope.opsSections.length; i++) {
  //   if ($state.includes($scope.opsSections[i].uiSref)) {
  //     $scope.activeSection = $scope.opsSections[i];
  //   } else {
  //     $log.debug("opsSections[" + i + "] is not the current state.");
  //   }
  // }
  //
  // for (var i = 0; i < $scope.databaseSections.length; i++) {
  //   if ($state.includes($scope.databaseSections[i].uiSref)) {
  //     $scope.activeSection = $scope.databaseSections[i];
  //   } else {
  //     $log.debug("databaseSections[" + i + "] is not the current state.");
  //   }
  // }
  //
  // $scope.setActiveSection = function(section) {
  //   $scope.activeSection = section;
  // };

});
