(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('SystemController', SystemController);

    SystemController.$inject = ['$scope', '$rootScope', '$state', '$log', 'siteService'];

    /* @ngInject */
    function SystemController($scope, $rootScope, $state, $log, siteService) {
      $log.debug("Loading SystemController");
      $log.debug("Welcome to the Admin experience. Remember: measure twice, cut once.");


      $scope.stateData = $state.current.data;

      $scope.siteAdminPages = [
        { pageTitle: "Dashboard", icon: "fa fa-table", uiSref: "app.system.site-admin.dashboard", active: false},
        { pageTitle: "Settings", icon: "fa fa-sliders", uiSref: "app.system.site-admin.site-settings", active: false},
        { pageTitle: "Tenants", icon: "fa fa-building-o", uiSref: "app.system.site-admin.tenants", active: false},
        { pageTitle: "Users", icon: "fa fa-users", uiSref: "app.system.site-admin.users", active: false},
        { pageTitle: "Status", icon: "fa fa-power-off", uiSref: "app.system.site-admin.site-status", active: false},
        { pageTitle: "Activity Log", icon: "fa fa-list-ul", uiSref: "app.system.site-admin.activity-log", active: false},
        { pageTitle: "Event Log", icon: "fa fa-list-ul", uiSref: "app.system.site-admin.event-log", active: false},
        { pageTitle: "Report Templates", icon: "fa fa-file-text-o", uiSref: "app.system.site-admin.report-templates", active: false},
        { pageTitle: "Library", icon: "fa fa-book", uiSref: "app.system.site-admin.library.occupations", active: false},
        { pageTitle: "Career Sets", icon: "fa fa-th-large", uiSref: "app.system.site-admin.career-sets", active: false}
      ];

      $scope.opsAdminPages = [
        { pageTitle: "Ops Dashboard", icon: "fa fa-table", uiSref: "app.system.ops-admin.dashboard", active: false},
        { pageTitle: "Ops Log", icon: "fa fa-list-ul", uiSref: "app.system.ops-admin.log", active: false},
        { pageTitle: "Diagnostics", icon: "fa fa-exclamation-triangle", uiSref: "app.system.ops-admin.diagnostics", active: false},
        { pageTitle: "Backup", icon: "fa fa-upload", uiSref: "app.system.ops-admin.backup", active: false}
      ];

      $scope.databaseAdminPages = [
        { pageTitle: "DB Dashboard", icon: "fa fa-table", uiSref: "app.system.database-admin.dashboard", active: false},
        { pageTitle: "Data Import", icon: "fa fa-upload", uiSref: "app.system.database-admin.data-import", active: false},
        { pageTitle: "Data Export", icon: "fa fa-download", uiSref: "app.system.database-admin.data-export", active: false},
        { pageTitle: "DB Log", icon: "fa fa-table", uiSref: "app.system.database-admin.log", active: false},
      ];

      $rootScope.$on('$stateChangeSuccess', function (ev, to, toParams, from, fromParams) {
        $scope.stateData = $state.current.data;
        setActiveNavItem();
      });


      $scope.goBack = function(){
        $state.go($scope.stateData.backState);
      };

      function setActiveNavItem() {

        for (var a = 0; a < $scope.siteAdminPages.length; a++) {
          if ($scope.siteAdminPages[a].pageTitle == $scope.stateData.pageTitle) {
            $scope.siteAdminPages[a].active = true;
          } else {
            $scope.siteAdminPages[a].active = false;
          }
        }

        for (var b = 0; b < $scope.opsAdminPages.length; b++) {
          if ($scope.opsAdminPages[b].pageTitle == $scope.stateData.pageTitle) {
            $scope.opsAdminPages[b].active = true;
          } else {
            $scope.opsAdminPages[b].active = false;
          }
        }

        for (var c = 0; c < $scope.databaseAdminPages.length; c++) {
          if ($scope.databaseAdminPages[c].pageTitle == $scope.stateData.pageTitle) {
            $scope.databaseAdminPages[c].active = true;
          } else {
            $scope.databaseAdminPages[c].active = false;
          }
        }

      }

      setActiveNavItem();

    }

})();
