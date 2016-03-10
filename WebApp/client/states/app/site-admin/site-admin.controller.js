app.controller('SiteAdminController', function ($scope, $rootScope, $state, $log, siteService, CONSTANTS) {
  $log.debug('Loading SiteAdminController...');

  $scope.stateData = $state.current.data;

  $scope.CONSTANTS = CONSTANTS;

  $scope.systemAdminPages = [
    { pageTitle: "Dashboard", icon: "fa fa-table", uiSref: "app.site-admin.system.dashboard", active: false},
    { pageTitle: "Tenants", icon: "fa fa-building-o", uiSref: "app.site-admin.system.tenants", active: false},
    { pageTitle: "Users", icon: "fa fa-users", uiSref: "app.site-admin.system.users", active: false},
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


});
